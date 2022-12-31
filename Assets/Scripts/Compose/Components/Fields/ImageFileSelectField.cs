using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Field for selecting an image file.
    /// Includes an image preview.
    /// </summary>
    public class ImageFileSelectField : FileSelectField
    {
        [SerializeField] private RawImage previewImage;
        [SerializeField] private GameObject hint;

        protected override void OnInvalidFilePath()
        {
            previewImage.gameObject.SetActive(false);
            hint.SetActive(true);
        }

        protected override void OnValidFilePath(FilePath path)
        {
            if (previewImage.texture != null)
            {
                Destroy(previewImage.texture);
            }

            previewImage.gameObject.SetActive(true);
            StartLoadingImage(path.FullPath).Forget();
            hint.SetActive(false);
        }

        protected new void Awake()
        {
            base.Awake();
            previewImage.texture = null;
            previewImage.gameObject.SetActive(false);
            hint.SetActive(true);
        }

        private async UniTask StartLoadingImage(string path)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + path))
            {
                await request.SendWebRequest();
                if (string.IsNullOrEmpty(request.error))
                {
                    Texture texture = DownloadHandlerTexture.GetContent(request);
                    previewImage.texture = texture;
                }
                else
                {
                    throw new IOException(I18n.S("Compose.Exception.LoadImage", new Dictionary<string, object>()
                    {
                        { "Path", path },
                        { "Error", request.error },
                    }));
                }
            }
        }
    }
}