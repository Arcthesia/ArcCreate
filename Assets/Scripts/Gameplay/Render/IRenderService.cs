using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public interface IRenderService
    {
        bool IsLoaded { get; }

        void DrawArcSegment(int colorId, bool highlight, Matrix4x4 matrix, ArcRenderProperties properties);

        void DrawArcShadow(Matrix4x4 matrix, ShadowRenderProperties properties);

        void DrawTraceSegment(Matrix4x4 matrix, ArcRenderProperties properties);

        void DrawTraceShadow(Matrix4x4 matrix, ShadowRenderProperties properties);

        void DrawArcHead(int colorId, bool highlight, Matrix4x4 matrix, ArcRenderProperties properties);

        void DrawTraceHead(Matrix4x4 matrix, ArcRenderProperties properties);

        void DrawArcCap(Texture texture, Matrix4x4 matrix, SpriteRenderProperties properties);

        void DrawArcTap(bool sfx, Texture texture, Matrix4x4 matrix, NoteRenderProperties properties);

        void DrawArcTapShadow(Matrix4x4 matrix, SpriteRenderProperties properties);

        void DrawConnectionLine(Matrix4x4 matrix, SpriteRenderProperties properties);

        void DrawHeightIndicator(Matrix4x4 matrix, SpriteRenderProperties properties);

        void DrawHold(Texture texture, Matrix4x4 matrix, LongNoteRenderProperties properties);

        void DrawTap(Texture texture, Matrix4x4 matrix, NoteRenderProperties properties);

        void SetTextures(Texture heightIndicator, Texture arctapShadow);

        void SetArcMaterials(List<Material> normal, List<Material> highlight);

        void SetShadowMaterial(Material material);

        void SetTraceMaterial(Material material);

        void UpdateRenderers();
    }
}