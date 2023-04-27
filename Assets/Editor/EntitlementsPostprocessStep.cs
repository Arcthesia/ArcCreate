// SPDX-FileCopyrightText: © 2021 Chris Marc Dailey (nitz)
// SPDX-License-Identifier: MIT

using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using System.IO;
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

public class EntitlementsPostprocessStep : MonoBehaviour
{
    [PostProcessBuild(2)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        switch (target)
        {
            case BuildTarget.iOS:
                UpdateInfoPlist(pathToBuiltProject);
                break;
            default:
                // nothing to do for this platform.
                return;
        }
    }

    private static void UpdateInfoPlist(string path)
    {
#if UNITY_IOS
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        PlistElementDict rootDict = plist.root;

        rootDict.SetString("Appearance", "Dark");
        rootDict.SetString("UIUserInterfaceStyle", "Dark");
        rootDict.SetBoolean("CADisableMinimumFrameDurationOnPhone", true);
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