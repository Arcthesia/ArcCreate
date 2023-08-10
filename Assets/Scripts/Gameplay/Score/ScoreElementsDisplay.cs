using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Score
{
    public class ScoreElementsDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private RectTransform scoreTextRect;
        [SerializeField] private TMP_Text scoreTitleText;
        [SerializeField] private GameObject predictiveDisplay;
        [SerializeField] private List<DisplayMode> displayModes;

        private void Awake()
        {
            Settings.ScoreDisplayMode.OnValueChanged.AddListener(OnScoreDisplayMode);
            OnScoreDisplayMode(Settings.ScoreDisplayMode.Value);
        }

        private void OnDestroy()
        {
            Settings.ScoreDisplayMode.OnValueChanged.RemoveListener(OnScoreDisplayMode);
        }

        private void OnScoreDisplayMode(int v)
        {
            ScoreDisplayMode mode = (ScoreDisplayMode)v;
            foreach (var m in displayModes)
            {
                if (mode == m.Mode)
                {
                    predictiveDisplay.SetActive(m.ShowPredictive);
                    scoreTitleText.text = m.TitleText;
                    scoreTextRect.offsetMin = new Vector2(m.OffsetMin, scoreTextRect.offsetMin.y);
                    scoreTextRect.offsetMax = new Vector2(m.OffsetMax, scoreTextRect.offsetMax.y);
                    scoreText.fontSize = m.FontSize;
                }
            }
        }

        [Serializable]
        private struct DisplayMode
        {
            public ScoreDisplayMode Mode;
            public string TitleText;
            public bool ShowPredictive;
            public float OffsetMin;
            public float OffsetMax;
            public float FontSize;
        }
    }
}