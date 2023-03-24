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
    /// Manager for scene transitioning. Allows for easy data transfer between scenes.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneRepresentative currentSceneRepresentative;
        private static string currentScene;
        private ITransition transition;
        private TransitionState transitionState = TransitionState.Idle;

        private SceneRepresentative loadingSceneRep;
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
            transition?.DisableGameObject();
            transition = newTransition;
        }

        /// <summary>
        /// Start the transition and switch to a new scene.
        /// Load the scene defined by sceneName, and unload the currently active scene.
        /// </summary>
        /// <param name="sceneName">The scene to switch to.</param>
        /// <param name="passData">Action for passing data between old and new scene.</param>
        /// <param name="onException">Action for when exception occurs while passing data.</param>
        /// <returns>UniTask instance.</returns>
        public async UniTask SwitchScene(string sceneName, Func<SceneRepresentative, UniTask> passData = null, Action<Exception> onException = null)
        {
            if (transitionState != TransitionState.Idle)
            {
                throw new Exception("Transition already in progress but another one is trying to start.");
            }

            transitionState = TransitionState.Starting;

            transition.EnableGameObject();
            await transition.StartTransition();

            transitionState = TransitionState.Waiting;
            UniTask waitTask = UniTask.Delay(transition.WaitDurationMs);
            SceneRepresentative rep = await LoadScene(sceneName);

            try
            {
                if (passData != null)
                {
                    await passData.Invoke(rep);
                }

                UnloadCurrentScene();
                currentScene = sceneName;
                currentSceneRepresentative = rep;
            }
            catch (Exception e)
            {
                onException?.Invoke(e);
                await SceneManager.UnloadSceneAsync(sceneName);
            }
            finally
            {
                await UniTask.WaitUntil(() => waitTask.Status == UniTaskStatus.Succeeded);
                transitionState = TransitionState.Ending;

                await UniTask.NextFrame();
                await transition.EndTransition();
                OnTransitionEnd?.Invoke();
                OnTransitionEnd = null;
                transition.DisableGameObject();
                transitionState = TransitionState.Idle;
            }
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
            await LoadScene(sceneName).ContinueWith((rep) =>
            {
                passData?.Invoke(rep);
                additivelyLoadedScenes.Add((sceneName, rep));
            });
        }

        /// <summary>
        /// Called by SceneRepresentative on awake, to notify that the scene has completely loaded.
        /// </summary>
        /// <param name="rep">The representative of the loaded scene.</param>
        public void LoadSceneComplete(SceneRepresentative rep)
        {
            loadingSceneRep = rep;
        }

        private void Awake()
        {
            Instance = this;
            LoadDefaultScene().Forget();
        }

        private async UniTask LoadDefaultScene()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                await I18n.StartLoadingLocale();
            }

            foreach (string scene in SceneNames.RequiredScenes)
            {
                SceneManager.LoadScene(scene, LoadSceneMode.Additive);
            }

            if (SceneManager.sceneCount == 1 + SceneNames.RequiredScenes.Length)
            {
                currentSceneRepresentative = await LoadScene(SceneNames.DefaultScene);
                currentScene = SceneNames.DefaultScene;
            }
            else
            {
                currentSceneRepresentative.OnNoBootScene();
            }
        }

        private async UniTask<SceneRepresentative> LoadScene(string sceneName)
        {
            loadingSceneRep = null;
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!load.isDone)
            {
                await UniTask.Yield();
            }

            while (loadingSceneRep == null)
            {
                await UniTask.Yield();
            }

            return loadingSceneRep;
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