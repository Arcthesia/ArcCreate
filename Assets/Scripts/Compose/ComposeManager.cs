using ArcCreate.Compose.Popups;
using ArcCreate.Gameplay;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Compose
{
    public class ComposeManager : SceneRepresentative
    {
        public override void OnNoBootScene()
        {
            LoadGameplayScene();
        }

        public override void OnUnloadScene()
        {
            Application.logMessageReceived -= OnLog;
        }

        protected override void OnSceneLoad()
        {
            Application.logMessageReceived += OnLog;
            LoadGameplayScene();
        }

        private void LoadGameplayScene()
        {
            var gameplayManager = FindObjectOfType<GameplayManager>();
            if (gameplayManager != null)
            {
                UseGameplay(gameplayManager);
                return;
            }

            if (SceneTransitionManager.Instance == null)
            {
                return;
            }

            SceneTransitionManager.Instance.LoadSceneAdditive(
                SceneNames.GameplayScene,
                rep =>
                {
                    var gameplayControl = rep as IGameplayControl;
                    UseGameplay(gameplayControl);
                }).Forget();
        }

        private void UseGameplay(IGameplayControl gameplay)
        {
            Services.Gameplay = gameplay ?? throw new System.Exception("Could not load gameplay scene");
            gameplay.EnablePauseMenu = false;
            gameplay.ShouldNotifyOnAudioEnd = false;
            gameplay.Chart.EnableColliderGeneration = true;
            TransitionScene.Instance.SetTargetCamera(gameplay.Camera.UICamera, "Topmost", 99);
        }

        private void OnLog(string condition, string stackTrace, LogType type)
        {
            Severity severity = Severity.Info;
            bool showStackTrace = true;
            switch (type)
            {
                case LogType.Warning:
                    severity = Severity.Warning;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    severity = Severity.Error;
                    break;
                default:
                    showStackTrace = false;
                    break;
            }

            Services.Popups.Notify(severity, showStackTrace ? $"{condition}\n{stackTrace}" : condition);
        }
    }
}