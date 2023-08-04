using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcCreate.SceneTransition
{
    // Code yoinked from ArcCore
    public class SceneRepresentative : MonoBehaviour
    {
        /// <summary>
        /// Lazy method for passing data in case cyclic dependency gets problematic.
        /// </summary>
        /// <param name="args">Argument to be passed.</param>
        public virtual void PassData(params object[] args)
        {
        }

        /// <summary>
        /// Called when the scene is unloaded.
        /// </summary>
        public virtual void OnUnloadScene()
        {
        }

        /// <summary>
        /// Called if the scene is started directly (i.e. started within the editor).
        /// For testing purposes only.
        /// </summary>
        public virtual void OnNoBootScene()
        {
        }

        protected IEnumerator EndOfFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action.Invoke();
        }

        /// <summary>
        /// Called after the scene transition is complete, i.e. after the shutter is open.
        /// </summary>
        protected virtual void OnTransitionComplete()
        {
        }

        /// <summary>
        /// Called when the scene is loaded.
        /// </summary>
        protected virtual void OnSceneLoad()
        {
        }

        private void Awake()
        {
            StartCoroutine(EndOfFrame(OnSceneLoad));
            if (SceneTransitionManager.Instance == null)
            {
                bool bootSceneFound = false;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.name == SceneNames.BootScene)
                    {
                        bootSceneFound = true;
                    }
                }

                if (!bootSceneFound)
                {
                    StartCoroutine(EndOfFrame(OnNoBootScene));
                    StartCoroutine(EndOfFrame(() => SceneManager.LoadSceneAsync(SceneNames.BootScene, LoadSceneMode.Additive)));
                }

                SceneTransitionManager.StartBootSceneDev(this);
                return;
            }

            NotifyManager();
        }

        private void NotifyManager()
        {
            SceneTransitionManager.Instance.LoadSceneComplete(this);
            SceneTransitionManager.Instance.OnTransitionEnd += OnTransitionComplete;
        }
    }
}
