// SPDX-FileCopyrightText: © 2021 Chris Marc Dailey (nitz)
// SPDX-License-Identifier: MIT

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class EntitlementsPostprocessStep : MonoBehaviour
{
#if UNITY_IOS
	private const string CodeSignEntitlementsPropertyName = "CODE_SIGN_ENTITLEMENTS";
	private const string EntitlementsPlistFilenameExtension = ".entitlements";
	private const string NetworkingMulticastEntitlementKey = "com.apple.developer.networking.multicast";
	private const bool NetworkingMulticastEntitlementValue = true;
#endif // UNITY_IOS

    [PostProcessBuild(2)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        switch (target)
        {
            case BuildTarget.iOS:
                PostProcessEntitlements(pathToBuiltProject);
                UpdateInfoPlist(pathToBuiltProject);
                break;
            default:
                // nothing to do for this platform.
                return;
        }
    }

    private static void PostProcessEntitlements(string pathToBuiltProject)
    {
#if UNITY_IOS
		// load project
		string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
		var project = new PBXProject();
		project.ReadFromFile(projectPath);
		string targetGuid = project.GetUnityMainTargetGuid();
		
		// create or modify the entitlements plist
		PlistDocument plist = new PlistDocument();
		string plistFilePath = project.GetEntitlementFilePathForTarget(targetGuid);
		string plistFileName;
		bool addEntitlementFile = false;
		
		// if we don't have an entitlements file already...
		if (string.IsNullOrEmpty(plistFilePath))
		{
			// ...get a path for a to create a new one.
			plistFileName = $"{Application.productName}{EntitlementsPlistFilenameExtension}";
			plistFilePath = Path.Combine(pathToBuiltProject, plistFileName);
			addEntitlementFile = true;
			plist.Create();
		}
		else
		{
			// ...just snag the basename from the path.
			plistFileName = Path.GetFileName(plistFilePath);
			plist.ReadFromFile(plistFilePath);
		}
		
		// modify the plist
		PlistElementDict root = plist.root;
		root.SetBoolean(NetworkingMulticastEntitlementKey, NetworkingMulticastEntitlementValue);
		
		// save the modified plist
		plist.WriteToFile(plistFilePath);
		Debug.Log($"Wrote Entitlements plist to {plistFilePath}");

		if (addEntitlementFile)
		{
			// add entitlements plist to project
			project.AddFile(plistFilePath, plistFileName);
			project.AddBuildProperty(targetGuid, CodeSignEntitlementsPropertyName, plistFilePath);
			project.WriteToFile(projectPath);
			Debug.Log($"Added Entitlements plist to project (target: {targetGuid})");
		}
#endif // UNITY_IOS
    }

    private static void UpdateInfoPlist(string path)
    {
#if UNITY_IOS
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        PlistElementDict rootDict = plist.root;

        PlistElementDict nsAppTransportSecurity = rootDict.CreateDict("NSAppTransportSecurity");
        nsAppTransportSecurity.SetBoolean("NSAllowsArbitraryLoads", true);
        nsAppTransportSecurity.SetBoolean("NSAllowsLocalNetworking", true);
        rootDict.SetString(
            "NSLocalNetworkUsageDescription",
            "ArcCreate requires local network for setting up connection with desktop client.");

        plist.WriteToFile(plistPath);
#endif // UNITY_IOS
    }
}