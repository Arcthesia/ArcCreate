#if UNITY_STANDALONE_WIN

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace SFB
{

    // For fullscreen support
    // - "PlayerSettings/Visible In Background" should be enabled, otherwise when file dialog opened app window minimizes automatically.
    public class StandaloneFileBrowserWindows : IStandaloneFileBrowser
    {

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {

            var shellFilters = GetShellFilterFromFileExtensionList(extensions);
            if (multiselect)
            {
                var paths = ShellFileDialogs.FileOpenDialog.ShowMultiSelectDialog(GetActiveWindow(), title, directory, string.Empty, shellFilters, null);
                return paths == null || paths.Count == 0 ? new string[0] : paths.ToArray();
            }
            else
            {
                var path = ShellFileDialogs.FileOpenDialog.ShowSingleSelectDialog(GetActiveWindow(), title, directory, string.Empty, shellFilters, null);
                return string.IsNullOrEmpty(path) ? new string[0] : new[] { path };
            }
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
        {
            cb.Invoke(OpenFilePanel(title, directory, extensions, multiselect));
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect)
        {

            var path = ShellFileDialogs.FolderBrowserDialog.ShowDialog(GetActiveWindow(), title, directory);
            return string.IsNullOrEmpty(path) ? new string[0] : new[] { path };
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb)
        {
            cb.Invoke(OpenFolderPanel(title, directory, multiselect));
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {

            var finalFilename = "";
            if (!string.IsNullOrEmpty(directory))
            {
                finalFilename = GetDirectoryPath(directory);
            }

            if (!string.IsNullOrEmpty(defaultName))
            {
                finalFilename += defaultName;
            }

            var shellFilters = GetShellFilterFromFileExtensionList(extensions);
            if (shellFilters.Length > 0 && !string.IsNullOrEmpty(finalFilename))
            {
                var extension = $".{shellFilters[0].Extensions[0]}";
                if (!string.IsNullOrEmpty(extension) && !finalFilename.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
                    finalFilename += extension;
            }

            var path = ShellFileDialogs.FileSaveDialog.ShowDialog(GetActiveWindow(), title, directory, finalFilename, shellFilters, 0);
            return path;
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
        {
            cb.Invoke(SaveFilePanel(title, directory, defaultName, extensions));
        }

        private static ShellFileDialogs.Filter[] GetShellFilterFromFileExtensionList(ExtensionFilter[] extensions)
        {
            var shellFilters = new List<ShellFileDialogs.Filter>();
            if (extensions != null)
            {
                foreach (var extension in extensions)
                {
                    if (extension.Extensions == null || extension.Extensions.Length == 0)
                        continue;

                    var displayName = extension.Name;
                    if (string.IsNullOrEmpty(displayName))
                    {
                        System.Text.StringBuilder extensionFormatted = new System.Text.StringBuilder();
                        foreach (var extensionStr in extension.Extensions)
                        {
                            if (extensionFormatted.Length > 0)
                                extensionFormatted.Append(";");
                            extensionFormatted.Append($"*.{extensionStr}");
                        }
                        displayName = $"({extensionFormatted})";
                    }
                    var filter = new ShellFileDialogs.Filter(displayName, extension.Extensions);
                    shellFilters.Add(filter);
                }
            }
            if (shellFilters.Count == 0)
            {
                shellFilters.AddRange(ShellFileDialogs.Filter.ParseWindowsFormsFilter(@"All files (*.*)|*.*"));
            }

            return shellFilters.ToArray();
        }

        private static string GetDirectoryPath(string directory)
        {
            var directoryPath = Path.GetFullPath(directory);
            if (!directoryPath.EndsWith("\\"))
            {
                directoryPath += "\\";
            }
            if (Path.GetPathRoot(directoryPath) == directoryPath)
            {
                return directory;
            }
            return Path.GetDirectoryName(directoryPath) + Path.DirectorySeparatorChar;
        }
    }
}

#endif