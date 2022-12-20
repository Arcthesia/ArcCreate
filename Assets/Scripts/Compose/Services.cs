using ArcCreate.Compose.Components;
using ArcCreate.Compose.Project;
using ArcCreate.Gameplay;
using UnityEngine;

namespace ArcCreate.Compose
{
    internal class Services : MonoBehaviour
    {
        [SerializeField] private ProjectService project;
        [SerializeField] private ColorPickerWindow colorPicker;

        public static IProjectService Project { get; private set; }

        public static IColorPickerService ColorPicker { get; private set; }

        public static IGameplayControl Gameplay { get; set; }

        private void Awake()
        {
            Project = project;
            ColorPicker = colorPicker;
        }
    }
}