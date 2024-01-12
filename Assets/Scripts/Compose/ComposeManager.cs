using System;
using System.IO;
using ArcCreate.Compose.Popups;
using ArcCreate.Gameplay;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Compose
{
    public class ComposeManager : SceneRepresentative
    {
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
                    StartCheckingStartupArgs();
                }).Forget();
        }

        private void UseGameplay(IGameplayControl gameplay)
        {
            Services.Gameplay = gameplay ?? throw new System.Exception("Could not load gameplay scene");
            gameplay.EnablePauseMenu = false;
            gameplay.ShouldNotifyOnAudioEnd = false;
            TransitionScene.Instance.SetTargetCamera(gameplay.Camera.UICamera, "Topmost", 99);
            TransitionScene.Instance.TriangleTileGameObject.SetActive(false);
            TransitionScene.Instance.UpdateCameraStatus();
        }

        private void StartCheckingStartupArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && File.Exists(args[1]) && args[1].EndsWith(".arcproj"))
            {
                Services.Project.OpenProject(args[1]);
            }
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