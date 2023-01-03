using ArcCreate.Gameplay;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcCreate.Compose
{
    public class ComposeManager : SceneRepresentative
    {
        [SerializeField] private RawImage gameplayView;

        public override void OnNoBootScene()
        {
            LoadGameplayScene();
        }

        protected override void OnSceneLoad()
        {
            LoadGameplayScene();
        }

        private void LoadGameplayScene()
        {
            if (SceneTransitionManager.Instance == null)
            {
                return;
            }

            SceneTransitionManager.Instance.LoadSceneAdditive(
                SceneNames.GameplayScene,
                rep =>
                {
                    var gameplayControl = rep as IGameplayControl;
                    Services.Gameplay = gameplayControl ?? throw new System.Exception("Could not load gameplay scene");
                    var renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
                    gameplayView.texture = renderTexture;
                    gameplayControl.SetTargetRenderTexture(renderTexture);
                    gameplayControl.ShouldUpdateInputSystem = false;
                }).Forget();
        }

        private void Update()
        {
            InputSystem.Update();
        }
    }
}