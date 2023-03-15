using ArcCreate.SceneTransition;
using UnityEngine.InputSystem;

namespace ArcCreate.Selection
{
    public class SelectionManager : SceneRepresentative
    {
        private void Update()
        {
            InputSystem.Update();
        }
    }
}