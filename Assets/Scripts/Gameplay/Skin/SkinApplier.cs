using ArcCreate.SceneTransition;
using ArcCreate.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Skin
{
    public class SkinApplier : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private SpriteSO jacketSO;
        [SerializeField] private Image background;
        [SerializeField] private SpriteRenderer videobackground;
        [SerializeField] private StringSO titleSO;
        [SerializeField] private StringSO composerSO;
        [SerializeField] private StringSO illustratorSO;
        [SerializeField] private StringSO charterSO;
        [SerializeField] private StringSO aliasSO;
        [SerializeField] private StringSO difficultySO;
        [SerializeField] private ColorSO difficultyColorSO;

        private void Awake()
        {
            gameplayData.Jacket.OnValueChange += OnJacket;
            gameplayData.Background.OnValueChange += OnBackground;
            gameplayData.DifficultyName.OnValueChange += OnDifficulty;
            gameplayData.DifficultyColor.OnValueChange += OnDifficultyColor;
            gameplayData.Title.OnValueChange += OnTitle;
            gameplayData.Composer.OnValueChange += OnComposer;
            gameplayData.Illustrator.OnValueChange += OnIllustrator;
            gameplayData.Charter.OnValueChange += OnCharter;
            gameplayData.Alias.OnValueChange += OnAlias;
        }

        private void OnDestroy()
        {
            gameplayData.Jacket.OnValueChange -= OnJacket;
            gameplayData.Background.OnValueChange -= OnBackground;
            gameplayData.DifficultyName.OnValueChange -= OnDifficulty;
            gameplayData.DifficultyColor.OnValueChange -= OnDifficultyColor;
            gameplayData.Title.OnValueChange -= OnTitle;
            gameplayData.Composer.OnValueChange -= OnComposer;
            gameplayData.Illustrator.OnValueChange -= OnIllustrator;
            gameplayData.Charter.OnValueChange -= OnCharter;
            gameplayData.Alias.OnValueChange -= OnAlias;
        }

        private void OnAlias(string value)
        {
            aliasSO.Value = value;
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

        private void OnDifficultyColor(Color value)
        {
            difficultyColorSO.Value = value;
        }

        private void OnDifficulty(string value)
        {
            difficultySO.Value = value;
        }

        private void OnBackground(Sprite value)
        {
            background.sprite = value;
            videobackground.sprite = value;
        }

        private void OnJacket(Sprite value)
        {
            jacketSO.Value = value;
        }
    }
}