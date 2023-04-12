using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public interface IRenderService
    {
        bool IsLoaded { get; }

        Mesh TapMesh { get; }

        Mesh HoldMesh { get; }

        Mesh ArcTapMesh { get; }

        void DrawArcCap(Texture texture, Matrix4x4 matrix, Color color);

        void DrawArcHead(int colorId, bool highlight, Matrix4x4 matrix, Color color, bool selected, float redValue, float y);

        void DrawArcSegment(int colorId, bool highlight, Matrix4x4 matrix, Color color, bool selected, float redValue, float y);

        void DrawArcShadow(Matrix4x4 matrix, Color color);

        void DrawArcTap(bool sfx, Texture texture, Matrix4x4 matrix, Color color, bool selected);

        void DrawArcTapShadow(Matrix4x4 matrix, Color color);

        void DrawConnectionLine(Matrix4x4 matrix, Color color);

        void DrawHeightIndicator(Matrix4x4 matrix, Color color);

        void DrawHold(Texture texture, Matrix4x4 matrix, Color color, bool selected, float from);

        void DrawTap(Texture texture, Matrix4x4 matrix, Color color, bool selected);

        void DrawTraceHead(Matrix4x4 matrix, Color color, bool selected);

        void DrawTraceSegment(Matrix4x4 matrix, Color color, bool selected);

        void DrawTraceShadow(Matrix4x4 matrix, Color color);

        void SetArcMaterial(Material normal, Material highlight);

        void SetShadowMaterial(Material material);

        void SetTextures(Texture heightIndicator, Texture arctapShadow);

        void SetTraceMaterial(Material material);

        void UpdateRenderers();
    }
}