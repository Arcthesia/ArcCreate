using SFB;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Utility
{
    public class Shell
    {
        public static string OpenFileDialog(string filterName, string[] extension, string title = "", string initPath = "")
        {
            ExtensionFilter filter = new ExtensionFilter()
            {
                Name = filterName,
                Extensions = extension,
            };

            return OpenFileDialog(filter, title, initPath);
        }

        public static string SaveFileDialog(string filterName, string[] extension, string title = "", string initPath = "", string defaultName = "")
        {
            ExtensionFilter filter = new ExtensionFilter()
            {
                Name = filterName,
                Extensions = extension,
            };

            return SaveFileDialog(filter, title, initPath, defaultName);
        }

        public static string OpenFileDialog(ExtensionFilter filter, string title = "", string initPath = "")
        {
            ExtensionFilter[] filters = new ExtensionFilter[] { filter };
            try
            {
                string[] strs = StandaloneFileBrowser.OpenFilePanel(title, initPath, filters, false);
                if (strs.Length > 0 && strs[0] != "")
                {
                    string str = strs[0];
                    if (str.StartsWith("file://"))
                    {
                        str = UnityWebRequest.UnEscapeURL(str.Replace("file://", ""));
                    }

                    return str;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void OpenExplorer(string selectPath)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                System.Diagnostics.Process.Start(selectPath?.Replace("/", "\\"));
            }
            else
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer) ? "xdg-open" : "open";
                p.StartInfo.Arguments = "\"" + selectPath + "\"";
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.WaitForExit();
                p.Close();
            }
        }

        public static string SaveFileDialog(ExtensionFilter filter, string title = "", string initPath = "", string defaultName = "")
        {
            ExtensionFilter[] filters = new ExtensionFilter[] { filter };
            try
            {
                string str = StandaloneFileBrowser.SaveFilePanel(title, initPath, defaultName, filters);
                if (str != "")
                {
                    if (str.StartsWith("file://"))
                    {
                        str = UnityWebRequest.UnEscapeURL(str.Replace("file://", ""));
                    }

                    return str;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
