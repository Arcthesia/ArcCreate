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
        /// Called when the scene is unloaded.
        /// </summary>
        public virtual void OnUnloadScene()
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

        /// <summary>
        /// Called if the scene is started directly (i.e. started within the editor).
        /// For testing purposes only.
        /// </summary>
        protected virtual void OnNoBootScene()
        {
        }

        private void Awake()
        {
            StartCoroutine(EndOfFrame(OnSceneLoad));
            if (SceneTransitionManager.Instance == null)
            {
                StartCoroutine(EndOfFrame(OnNoBootScene));
                StartCoroutine(EndOfFrame(() => SceneManager.LoadSceneAsync(SceneNames.BootScene, LoadSceneMode.Additive)));
                SceneTransitionManager.StartBootSceneDev(this);
                return;
            }

            SceneTransitionManager.Instance.LoadSceneComplete(this);
            SceneTransitionManager.Instance.OnTransitionEnd += OnTransitionComplete;
        }
    }
}
