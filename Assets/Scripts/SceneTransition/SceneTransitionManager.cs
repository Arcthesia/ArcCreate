using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcCreate.SceneTransition
{
    public enum TransitionState
    {
        Idle,
        Ending,
        Starting,
        Waiting,
        ReadyToEnd,
    }

    // Code yoinked from ArcCore

    /// <summary>
    /// Manager for shutter and scene transition. Allows for easy data transfer between scenes.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneRepresentative currentSceneRepresentative;
        private static string currentScene;
        private ITransition transition;
        private TransitionState transitionState = TransitionState.Idle;

        private Action onTransitionWait;
        private Action<SceneRepresentative> onLoadSceneComplete;
        private readonly List<(string sceneName, SceneRepresentative representative)> additivelyLoadedScenes = new List<(string, SceneRepresentative)>();

        public static SceneTransitionManager Instance { get; private set; }

        public Action OnTransitionEnd { get; set; }

        public TransitionState TransitionState => transitionState;

        /// <summary>
        /// Called if game started without boot scene.
        /// </summary>
        /// <param name="rep">The representative to be set as active.</param>
        public static void StartBootSceneDev(SceneRepresentative rep)
        {
            currentSceneRepresentative = rep;
            currentScene = SceneManager.GetActiveScene().name;
        }

        public void SetTransition(ITransition newTransition)
        {
            transition.DisableGameObject();
            transition = newTransition;
        }

        /// <summary>
        /// Start the transition and switch to a new scene.
        /// Load the scene defined by sceneName, and unload the currently active scene.
        /// </summary>
        /// <param name="sceneName">The scene to switch to.</param>
        /// <param name="passData">Action for passing data between old and new scene.</param>
        public void SwitchScene(string sceneName, Action<SceneRepresentative> passData = null)
        {
            if (transitionState != TransitionState.Idle)
            {
                throw new Exception("Transition already in progress but another one is trying to start.");
            }

            onLoadSceneComplete += (rep) =>
            {
                passData?.Invoke(rep);

                UnloadCurrentScene();

                currentScene = sceneName;
                currentSceneRepresentative = rep;

                transitionState = TransitionState.ReadyToEnd;
            };
            onTransitionWait += () => LoadScene(sceneName).Forget();

            StartTransition().Forget();
        }

        /// <summary>
        /// Additively load a new scene.
        /// All additively loaded scene are destroyed along with the currently active scene.
        /// </summary>
        /// <param name="sceneName">The scene to load.</param>
        /// <param name="passData">Action for passing data between scenes.</param>
        /// <returns>Unitask instance.</returns>
        public async UniTask LoadSceneAdditive(string sceneName, Action<SceneRepresentative> passData = null)
        {
            onLoadSceneComplete += (rep) =>
            {
                passData?.Invoke(rep);
                additivelyLoadedScenes.Add((sceneName, rep));
            };
            await LoadScene(sceneName);
        }

        /// <summary>
        /// Called by SceneRepresentative on awake, to notify that the scene has completely loaded.
        /// </summary>
        /// <param name="rep">The representative of the loaded scene.</param>
        public void LoadSceneComplete(SceneRepresentative rep)
        {
            onLoadSceneComplete.Invoke(rep);
            onLoadSceneComplete = null;
        }

        /// <summary>
        /// Start the transition without switching scene.
        /// </summary>
        /// <param name="showInfo">Whether or not to show the information on shutter.</param>
        /// <returns>Unitask instance.</returns>
        public async UniTask StartTransitionWithoutSwitchingScene()
        {
            transitionState = TransitionState.Starting;
            transition.EnableGameObject();
            await transition.StartTransition();
            transitionState = TransitionState.Waiting;
        }

        /// <summary>
        /// End the transition without switching scene.
        /// </summary>
        /// <returns>Unitask instance.</returns>
        public async UniTask EndTransitionWithoutSwitchingScene()
        {
            transitionState = TransitionState.Ending;
            await transition.EndTransition();
            transition.DisableGameObject();
            transitionState = TransitionState.Idle;
        }

        private void Awake()
        {
            Instance = this;
            if (SceneManager.sceneCount == 1)
            {
                SceneManager.LoadScene(SceneNames.DefaultScene, LoadSceneMode.Additive);
            }
        }

        private void OnShutterCloseCallback()
        {
            onTransitionWait?.Invoke();
            onTransitionWait = null;
        }

        private void OnShutterOpenCallback()
        {
            OnTransitionEnd?.Invoke();
            OnTransitionEnd = null;
        }

        private async UniTask StartTransition()
        {
            transitionState = TransitionState.Starting;

            transition.EnableGameObject();
            await transition.StartTransition();
            OnShutterCloseCallback();

            transitionState = TransitionState.Waiting;
            await UniTask.Delay(transition.WaitDurationMs);

            while (transitionState != TransitionState.ReadyToEnd)
            {
                await UniTask.Yield();
            }

            await EndTransition();
        }

        private async UniTask EndTransition()
        {
            transitionState = TransitionState.Ending;

            await transition.EndTransition();
            OnShutterOpenCallback();
            transition.DisableGameObject();
            transitionState = TransitionState.Idle;
        }

        private async UniTask LoadScene(string sceneName)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!load.isDone)
            {
                await UniTask.Yield();
            }
        }

        private void UnloadCurrentScene()
        {
            foreach (var (sceneName, representative) in additivelyLoadedScenes)
            {
                representative.OnUnloadScene();
                SceneManager.UnloadSceneAsync(sceneName);
            }

            additivelyLoadedScenes.Clear();

            if (currentSceneRepresentative != null)
            {
                currentSceneRepresentative.OnUnloadScene();
            }

            if (currentScene != null)
            {
                SceneManager.UnloadSceneAsync(currentScene);
            }
        }
    }
}