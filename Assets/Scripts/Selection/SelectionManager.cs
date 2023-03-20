using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace ArcCreate.Selection
{
    public class SelectionManager : SceneRepresentative
    {
        public override void OnNoBootScene()
        {
            SceneTransitionManager.Instance.LoadSceneAdditive(SceneNames.StorageScene).Forget();
        }

        private void Update()
        {
            InputSystem.Update();
        }
    }
}