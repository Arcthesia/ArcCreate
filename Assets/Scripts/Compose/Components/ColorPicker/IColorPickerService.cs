using System;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    public interface IColorPickerService
    {
        Action<Color> OnColorChanged { get; set; }

        void OpenAt(Vector2 screenPosition, Color defaultColor);
    }
}