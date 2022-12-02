using ArcCreate.Gameplay.Judgement;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Score
{
    public class ScoreService : MonoBehaviour, IScoreService
    {
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text scoreText;

        public void ProcessJudgement(JudgementResult result)
        {
        }

        public void ResetScoreTo(int currentCombo, int totalCombo)
        {
        }
    }
}