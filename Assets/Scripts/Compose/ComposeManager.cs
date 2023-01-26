using System;
using ArcCreate.Compose.Popups;
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

        public override void OnUnloadScene()
        {
            Application.logMessageReceived -= OnLog;
        }

        protected override void OnSceneLoad()
        {
            LoadGameplayScene();
            Application.logMessageReceived += OnLog;
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
                    gameplayControl.ShouldUpdateInputSystem = false;
                    Debug.Log(I18n.S("Compose.Notify.GameplayLoaded"));
                }).Forget();
        }

        private void Update()
        {
            InputSystem.Update();
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