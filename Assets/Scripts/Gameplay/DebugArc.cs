using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Gameplay
{
    public class DebugArc : MonoBehaviour
    {
        [SerializeField] private LineRenderer[] lineToFinger;

        [SerializeField] private Image[] arcsExistIndicator;

        public static DebugArc Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}