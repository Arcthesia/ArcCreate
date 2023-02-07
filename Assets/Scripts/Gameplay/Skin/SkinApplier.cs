using System;
using ArcCreate.SceneTransition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ArcCreate.Gameplay.Skin
{
    public class SkinApplier : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private SpriteSO jacketSO;
        [SerializeField] private Image background;
        [SerializeField] private GameObject videoBackgroundRenderer;
        [SerializeField] private VideoPlayer videoBackground;
        [SerializeField] private StringSO titleSO;
        [SerializeField] private StringSO composerSO;
        [SerializeField] private StringSO illustratorSO;
        [SerializeField] private StringSO charterSO;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private Image difficultyColor;

        private void Awake()
        {
            gameplayData.Jacket.OnValueChange += OnJacket;
            gameplayData.Background.OnValueChange += OnBackground;
            gameplayData.DifficultyName.OnValueChange += OnDifficulty;
            gameplayData.DifficultyColor.OnValueChange += OnDifficultyColor;
            gameplayData.VideoBackgroundUrl.OnValueChange += OnVideoBackground;
            gameplayData.Title.OnValueChange += OnTitle;
            gameplayData.Composer.OnValueChange += OnComposer;
            gameplayData.Illustrator.OnValueChange += OnIllustrator;
            gameplayData.Charter.OnValueChange += OnCharter;
        }

        private void OnDestroy()
        {
            gameplayData.Jacket.OnValueChange -= OnJacket;
            gameplayData.Background.OnValueChange -= OnBackground;
            gameplayData.DifficultyName.OnValueChange -= OnDifficulty;
            gameplayData.DifficultyColor.OnValueChange -= OnDifficultyColor;
            gameplayData.VideoBackgroundUrl.OnValueChange -= OnVideoBackground;
            gameplayData.Title.OnValueChange -= OnTitle;
            gameplayData.Composer.OnValueChange -= OnComposer;
            gameplayData.Illustrator.OnValueChange -= OnIllustrator;
            gameplayData.Charter.OnValueChange -= OnCharter;
        }

        private void OnCharter(string value)
        {
            charterSO.Value = value;
        }

        private void OnIllustrator(string value)
        {
            illustratorSO.Value = value;
        }

        private void OnComposer(string value)
        {
            composerSO.Value = value;
        }

        private void OnTitle(string value)
        {
            titleSO.Value = value;
        }

        private void OnVideoBackground(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                videoBackground.url = value;
            }

            videoBackgroundRenderer.SetActive(string.IsNullOrEmpty(value));
        }

        private void OnDifficultyColor(Color value)
        {
            difficultyColor.color = value;
        }

        private void OnDifficulty(string value)
        {
            difficultyText.text = value;
        }

        private void OnBackground(Sprite value)
        {
            background.sprite = value;
        }

        private void OnJacket(Sprite value)
        {
            jacketSO.Value = value;
        }
    }
}