using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Utility
{
    [RequireComponent(typeof(TMP_Text))]
    public class VersionDisplay : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<TMP_Text>().text = I18n.S("Compose.UI.Startup.Version", Application.version);
        }
    }
}