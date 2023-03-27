using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Animation;
using Cysharp.Threading.Tasks;
#if UNITY_IOS || UNITY_ANDROID
using NativeFilePickerNamespace;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ArcCreate.Storage
{
    /// <summary>
    /// Partial class for handling file IO while importing.
    /// </summary>
    public partial class FileImportManager : MonoBehaviour
    {
        private const int InternalPackageVersion = 0;

        [SerializeField] private StorageData storageData;

        [Header("Conflict")]
        [SerializeField] private TMP_Text replaceWithVersion;
        [SerializeField] private TMP_Text originalVersion;
        [SerializeField] private TMP_Text replaceWithIdentifier;
        [SerializeField] private TMP_Text originalIdentifier;
        [SerializeField] private Button replaceButton;
        [SerializeField] private Button keepOriginalButton;

        [Header("Loading")]
        [SerializeField] private TMP_Text loadingText;

        [Header("Exception")]
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private Button errorConfirmButton;

        [Header("Summary")]
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private Button summaryConfirmButton;
        [SerializeField] private GameObject noAssetImportedIndicator;
        [SerializeField] private GameObject assetsImportedIndicator;

        [Header("Animation")]
        [SerializeField] private ScriptedAnimator loadingAnimator;
        [SerializeField] private ScriptedAnimator conflictAnimator;
        [SerializeField] private ScriptedAnimator errorAnimator;
        [SerializeField] private ScriptedAnimator summaryAnimator;

        private bool replacePressed;
        private bool keepPressed;

        private bool ShouldStorePackages => Application.isEditor || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android;

        private void LoadDatabase()
        {
            if (!ShouldStorePackages)
            {
                return;
            }

            // Import default arcpkg if db does not exist
            bool dbExists = File.Exists(FileStatics.DatabasePath);
            bool outdatedInternalPackage = PlayerPrefs.GetInt("CurrentInternalPackageVersion", -1) < InternalPackageVersion;
            if (!dbExists || outdatedInternalPackage)
            {
                if (Directory.Exists(FileStatics.FileStoragePath))
                {
                    Directory.Delete(FileStatics.FileStoragePath, true);
                }

                Database.Initialize();
                if (Application.platform == RuntimePlatform.Android)
                {
                    Debug.Log("Detected Android platform. Using UnityWebRequest to fetch default package");
                    LoadDefaultPackageAndroid(FileStatics.DefaultPackagePath).Forget();
                }
                else
                {
                    ImportArchive(FileStatics.DefaultPackagePath, true).Forget();
                }

                PlayerPrefs.SetInt("CurrentInternalPackageVersion", InternalPackageVersion);
            }
            else
            {
                Debug.Log("db file exists at " + FileStatics.DatabasePath);
                Database.Initialize();
                storageData.NotifyStorageChange();
            }
        }

        private async UniTask LoadDefaultPackageAndroid(string path)
        {
            // Fetch data from within obb with UnityWebRequest
            using (UnityWebRequest req = UnityWebRequest.Get(path))
            {
                await req.SendWebRequest();

                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    throw new Exception($"Cannot load default package");
                }

                // Copy to temporary path
                byte[] data = req.downloadHandler.data;
                string copyPath = Path.Combine(FileStatics.TempPath, FileStatics.DefaultPackage);
                if (!Directory.Exists(Path.GetDirectoryName(copyPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(copyPath));
                }

                using (FileStream fs = new FileStream(copyPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                }

                // Import copied file
                await ImportArchive(copyPath, true);
                File.Delete(copyPath);
            }
        }

        private async UniTask<bool> PromptConflictToUser(IStorageUnit replaceWith, IStorageUnit original)
        {
            replaceWithVersion.text = I18n.S("Storage.ImportConflict.Version", replaceWith.Version);
            originalVersion.text = I18n.S("Storage.ImportConflict.Version", original.Version);
            replaceWithIdentifier.text = I18n.S("Storage.ImportConflict.Identifier", replaceWith.Identifier);
            originalIdentifier.text = I18n.S("Storage.ImportConflict.Identifier", original.Identifier);

            await UniTask.NextFrame();
            loadingAnimator.Hide();
            conflictAnimator.Show();
            replacePressed = false;
            keepPressed = false;
            await UniTask.WaitUntil(() => replacePressed || keepPressed);
            conflictAnimator.Hide();
            loadingAnimator.Show();
            await UniTask.Delay((int)(conflictAnimator.Length * 1000));

            return replacePressed;
        }

        private void Awake()
        {
            if (ShouldStorePackages)
            {
                keepOriginalButton.onClick.AddListener(() => keepPressed = true);
                replaceButton.onClick.AddListener(() => replacePressed = true);
                errorConfirmButton.onClick.AddListener(HideError);
                summaryConfirmButton.onClick.AddListener(HideSummary);
                storageData.OnOpenFilePicker += ImportPackageFromFilePicker;
                LoadDatabase();

                Application.focusChanged += CheckPackageImport;
                CheckPackageImportInPersistentDirectory().Forget();
            }
        }

        private void OnDestroy()
        {
            if (ShouldStorePackages)
            {
                keepOriginalButton.onClick.RemoveAllListeners();
                replaceButton.onClick.RemoveAllListeners();
                errorConfirmButton.onClick.RemoveListener(HideError);
                summaryConfirmButton.onClick.RemoveListener(HideSummary);
                storageData.OnOpenFilePicker -= ImportPackageFromFilePicker;

                Application.focusChanged -= CheckPackageImport;
                Database.Dispose();
            }
        }

        private void ImportPackageFromFilePicker()
        {
#if UNITY_IOS
            string[] args = new string[] { ".arcpkg", "public.data", "public.archive" };
#elif UNITY_ANDROID
            string[] args = new string[] { ".arcpkg", "image/*", "application/*" };
#endif

#if UNITY_IOS || UNITY_ANDROID
            try
            {
                if (NativeFilePicker.IsFilePickerBusy())
                {
                    return;
                }

                // Pick a file
                NativeFilePicker.Permission permission = NativeFilePicker.PickFile(
                    (path) =>
                    {
                        if (path == null)
                        {
                            Debug.Log("Operation cancelled");
                        }
                        else
                        {
                            ImportArchive(path).Forget();
                        }
                    }, args);

                Debug.Log("Permission result: " + permission);
            }
            catch (Exception e)
            {
                DisplayError("Package", e);
                Debug.LogError(e);
            }
#endif
        }

        private void CheckPackageImport(bool focus)
        {
#if UNITY_ANDROID
            try
            {
                var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject intent = context.Call<AndroidJavaObject>("getIntent");
                string action = intent?.Call<string>("getAction");
                if (action == "android.intent.action.VIEW")
                {
                    string tempPath = Path.Combine(FileStatics.TempPath, "import_temp.arcpkg");
                    AndroidJavaObject contentResolver = context.Call<AndroidJavaObject>("getContentResolver");
                    AndroidJavaObject uri = intent.Call<AndroidJavaObject>("getData");
                    AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uri);

                    string path = uri.Call<string>("getPath");
                    var helper = new AndroidJavaObject("com.Arcthesia.ArcCreate.ArcCreateHelper");

                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }

                    if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
                    }

                    string error = helper.Call<string>("processInputStream", inputStream, tempPath);
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    ImportArchive(tempPath).Forget();
                    context.Call("setIntent", null);
                }
            }
            catch (Exception e)
            {
                DisplayError("Package", e);
                Debug.LogError(e);
            }
#endif
        }

        private async UniTask CheckPackageImportInPersistentDirectory()
        {
            await UniTask.DelayFrame(10);
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(FileStatics.CheckImportPath));
            if (!dir.Exists)
            {
                Directory.CreateDirectory(dir.FullName);
                return;
            }

            string error = string.Empty;

            List<FileInfo> imported = new List<FileInfo>();
            foreach (FileInfo fileInfo in dir.EnumerateFiles())
            {
                print(fileInfo.FullName);
                if (fileInfo.Extension == ".arcpkg")
                {
                    try
                    {
                        await ImportArchive(fileInfo.FullName);
                        imported.Add(fileInfo);
                    }
                    catch (Exception e)
                    {
                        error += e.Message + "\n";
                    }
                }
            }

            foreach (FileInfo fileInfo in imported)
            {
                fileInfo.CopyTo(fileInfo.FullName + ".imported");
                fileInfo.Delete();
            }

            if (!string.IsNullOrEmpty(error))
            {
                DisplayError("Package", new Exception(error));
            }
        }

        private void ClearError()
        {
            errorText.text = string.Empty;
            errorAnimator.gameObject.SetActive(false);
        }

        private void DisplayError(string target, Exception e)
        {
            if (string.IsNullOrEmpty(errorText.text))
            {
                errorText.text = $"Import target: {target}\nException:\n{e.Message}";
            }
            else
            {
                errorText.text += $"\n---\nImport target: {target}\nException:\n{e.Message}";
            }

            if (!errorAnimator.gameObject.activeSelf)
            {
                HideLoading();
                errorAnimator.Show();
            }
        }

        private void ClearSummary()
        {
            summaryText.text = string.Empty;
            summaryAnimator.gameObject.SetActive(false);
        }

        private void ShowSummary(List<IStorageUnit> assets)
        {
            if (assets == null || assets.Count == 0)
            {
                noAssetImportedIndicator.SetActive(true);
                assetsImportedIndicator.SetActive(false);
            }
            else
            {
                noAssetImportedIndicator.SetActive(false);
                assetsImportedIndicator.SetActive(true);
                StringBuilder sb = new StringBuilder();
                foreach (var asset in assets)
                {
                    sb.Append($"{asset.Type}: {asset.Identifier}\n");
                }

                summaryText.text = sb.ToString();
            }

            if (!summaryAnimator.gameObject.activeSelf && !errorAnimator.gameObject.activeSelf)
            {
                HideLoading();
                summaryAnimator.Show();
            }
        }

        private void ShowLoading(string task)
        {
            loadingText.text = I18n.S("Storage.Loading.Message", task);
            if (!loadingAnimator.gameObject.activeSelf)
            {
                loadingAnimator.Show();
            }
        }

        private void HideError()
        {
            errorAnimator.Hide();
            if (!string.IsNullOrEmpty(summaryText.text))
            {
                summaryAnimator.Show();
            }
        }

        private void HideSummary()
        {
            summaryAnimator.Hide();
        }

        private void HideLoading()
        {
            if (loadingAnimator.gameObject.activeSelf)
            {
                loadingAnimator.Hide();
            }
        }
    }
}