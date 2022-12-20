using System;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    public class ArcColorInputField : MonoBehaviour
    {
        [SerializeField] private ColorInputField highColor;
        [SerializeField] private ColorInputField lowColor;

        public event Action<(Color high, Color low)> OnValueChange;

        private void Awake()
        {
            highColor.OnValueChange += OnChange;
            lowColor.OnValueChange += OnChange;
        }

        private void OnChange(Color obj)
        {
            OnValueChange?.Invoke((highColor.Value, lowColor.Value));
        }
    }
}