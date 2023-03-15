using System;
using System.IO;
using ArcCreate.Storage.Data;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private TMP_Text replaceWithCreatedAt;
        [SerializeField] private TMP_Text originalCreatedAt;
        [SerializeField] private TMP_Text replaceWithIdentifier;
        [SerializeField] private TMP_Text originalIdentifier;
        [SerializeField] private Button replaceButton;
        [SerializeField] private Button keepOriginalButton;

        [Header("Animation")]
        [SerializeField] private RectTransform conflictNotifyRect;
        [SerializeField] private CanvasGroup conflictNotifyCanvas;
        [SerializeField] private Vector3 animationStartScale;
        [SerializeField] private float animationScaleDuration;
        [SerializeField] private float animationAlphaDuration;
        [SerializeField] private Ease animationScaleEasing;
        [SerializeField] private Ease animationAlphaEasing;

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
            if (!File.Exists(FileStatics.DatabasePath))
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
                    ImportArchive(FileStatics.DefaultPackagePath).Forget();
                }
            }
            else
            {
                Debug.Log("db file exists at " + FileStatics.DatabasePath);
                Database.Initialize();
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
                using (FileStream fs = new FileStream(copyPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                }

                // Import copied file
                await ImportArchive(copyPath);
                File.Delete(copyPath);
            }
        }

        private async UniTask<bool> PromptConflictToUser(IStorageUnit replaceWith, IStorageUnit original)
        {
            replaceWithCreatedAt.text = TimeZoneInfo.ConvertTimeFromUtc(replaceWith.CreatedAt, TimeZoneInfo.Local).ToShortDateString();
            originalCreatedAt.text = TimeZoneInfo.ConvertTimeFromUtc(original.CreatedAt, TimeZoneInfo.Local).ToShortDateString();
            replaceWithIdentifier.text = replaceWith.Identifier;
            originalIdentifier.text = original.Identifier;

            AnimateCanvasAppear();

            replacePressed = false;
            keepPressed = false;
            await UniTask.WaitUntil(() => replacePressed || keepPressed);

            AnimateCanvasDisappear();

            return replacePressed;
        }

        private void AnimateCanvasAppear()
        {
            conflictNotifyRect.gameObject.SetActive(true);
            conflictNotifyRect.DOScale(Vector3.one, animationScaleDuration).SetEase(animationScaleEasing);
            conflictNotifyCanvas.alpha = 0;
            conflictNotifyCanvas.DOFade(1, animationAlphaDuration).SetEase(animationAlphaEasing);
        }

        private void AnimateCanvasDisappear()
        {
            conflictNotifyRect.DOScale(animationStartScale, animationScaleDuration).SetEase(animationScaleEasing);
            conflictNotifyCanvas.DOFade(0, animationAlphaDuration).SetEase(animationAlphaEasing)
                .OnComplete(() => conflictNotifyRect.gameObject.SetActive(false));
        }

        private void Awake()
        {
            if (ShouldStorePackages)
            {
                keepOriginalButton.onClick.AddListener(() => keepPressed = true);
                replaceButton.onClick.AddListener(() => replacePressed = true);
                LoadDatabase();

                Application.focusChanged += CheckPackageImport;
            }
        }

        private void OnDestroy()
        {
            if (ShouldStorePackages)
            {
                keepOriginalButton.onClick.RemoveAllListeners();
                replaceButton.onClick.RemoveAllListeners();

                Application.focusChanged -= CheckPackageImport;
            }
        }

        private void CheckPackageImport(bool focus)
        {
            try
            {
                var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject intent = context.Call<AndroidJavaObject>("getIntent");
                string action = intent.Call<string>("getAction");
                if (action == "android.intent.action.VIEW")
                {
                    AndroidJavaObject uri = intent.Call<AndroidJavaObject>("getData");
                    string path = uri.Call<string>("getPath");
                    ImportArchive(path).Forget();
                }
            }
            catch
            {
            }
        }
    }
}