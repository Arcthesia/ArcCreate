using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Utility
{
    [RequireComponent(typeof(Button))]
    public class LinkButton : MonoBehaviour
    {
        [SerializeField] private string targetLink;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(GoToTargetLink);
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(GoToTargetLink);
        }

        private void GoToTargetLink()
        {
            Application.OpenURL(targetLink);
        }
    }
}