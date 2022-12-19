using ArcCreate.SceneTransition;
using UnityEngine.InputSystem;

namespace ArcCreate.Compose
{
    public class ComposeManager : SceneRepresentative
    {
        private void Update()
        {
            InputSystem.Update();
        }
    }
}