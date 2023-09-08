using System;
using Google.MaterialDesign.Icons;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ArcCreate.Selection.Interface
{
    public class StarRating : MonoBehaviour
    {
        [SerializeField] private MaterialIcon[] icons;
        [SerializeField] private string filledIconCode;
        [SerializeField] private string emptyIconCode;

        private int value;

        public event Action<int> OnValueChanged;

        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged?.Invoke(value);
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].iconUnicode = i < value ? filledIconCode : emptyIconCode;
            }
        }
    }
}