using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Remote.Gameplay
{
    public class RemoteDeviceRow : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text ipText;
        [SerializeField] private TMP_Text messageText;
        private RemoteManager remoteManager;
        private IPAddress ipAddress;
        private string message;

        public IPAddress IPAddress => ipAddress;

        public void Setup(IPAddress ipAddress, string message, RemoteManager remoteManager)
        {
            this.remoteManager = remoteManager;
            this.ipAddress = ipAddress;
            this.message = message;
            ipText.text = $"IP:{ipAddress}";
            messageText.text = message;
            button.onClick.AddListener(OnClick);
        }

        public void SetMessage(string message)
        {
            this.message = message;
            messageText.text = message;
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (remoteManager != null)
            {
                remoteManager.ConnectTo(ipAddress, message);
            }
        }
    }
}