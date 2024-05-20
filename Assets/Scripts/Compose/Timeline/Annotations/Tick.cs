using ArcCreate.Utility.Extension;
using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Timeline
{
    public class Tick : MonoBehaviour
    {
        private int currentTiming = int.MinValue;
        private static readonly char[] ArrayChar = new char[16];

        private RectTransform rect;
        [SerializeField] private TMP_Text text;

        public void SetTick(float x, int timing)
        {
            if (currentTiming != timing)
            {
                ArrayChar.SetNumberDigitsToArray(timing, out int length);
                text.SetCharArray(ArrayChar, ArrayChar.Length - length, length);
            }

            currentTiming = timing;
            rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }
    }
}