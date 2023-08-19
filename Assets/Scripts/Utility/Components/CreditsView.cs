using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Selection.Interface
{
    [RequireComponent(typeof(TMP_Text))]
    public class CreditsView : MonoBehaviour
    {
        private static readonly string CreditsPath = Path.Combine(Application.streamingAssetsPath, "credits.txt");

        private void Awake()
        {
            I18n.OnLocaleChanged += Reload;
            Reload();
        }

        private void OnDestroy()
        {
            I18n.OnLocaleChanged -= Reload;
        }

        private void Reload()
        {
            StartReloading().Forget();
        }

        private async UniTask StartReloading()
        {
            TMP_Text text = GetComponent<TMP_Text>();
            string data = string.Empty;
            if (Application.platform == RuntimePlatform.Android)
            {
                using (var req = UnityWebRequest.Get(CreditsPath))
                {
                    await req.SendWebRequest();
                    if (!string.IsNullOrEmpty(req.error))
                    {
                        Debug.LogWarning($"Couldn't load credit file at {CreditsPath}");
                        return;
                    }

                    data = req.downloadHandler.text;
                }
            }
            else
            {
                data = File.ReadAllText(CreditsPath);
            }

            data = Regex.Replace(data, @"{([^}]+)}", m => I18n.S(m.Groups[1].Value));
            data = Regex.Replace(data, @"\[([^\]]+)\]", m => I18n.GetLocalName(m.Groups[1].Value));
            text.text = data;
        }
    }
}