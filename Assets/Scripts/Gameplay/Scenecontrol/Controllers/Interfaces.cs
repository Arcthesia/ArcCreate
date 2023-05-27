using UnityEngine;

#pragma warning disable
namespace ArcCreate.Gameplay.Scenecontrol
{
    public interface IController
    {
        ValueChannel Active { get; set; }

        bool DefaultActive { get; }
    }

    public interface INoteIndividualController : IController
    {
        int GroupNumber { get; }
    }

    public interface IPositionController : IController
    {
        bool EnablePositionModule { get; set; }

        ValueChannel TranslationX { get; set; }

        ValueChannel TranslationY { get; set; }

        ValueChannel TranslationZ { get; set; }

        ValueChannel RotationX { get; set; }

        ValueChannel RotationY { get; set; }

        ValueChannel RotationZ { get; set; }

        ValueChannel ScaleX { get; set; }

        ValueChannel ScaleY { get; set; }

        ValueChannel ScaleZ { get; set; }

        Vector3 DefaultTranslation { get; }

        Quaternion DefaultRotation { get; }

        Vector3 DefaultScale { get; }

        void UpdatePosition(Vector3 translation, Quaternion rotation, Vector3 scale);
    }

    public interface IColorController : IController
    {
        bool EnableColorModule { get; set; }

        ValueChannel ColorR { get; set; }

        ValueChannel ColorG { get; set; }

        ValueChannel ColorB { get; set; }

        ValueChannel ColorH { get; set; }

        ValueChannel ColorS { get; set; }

        ValueChannel ColorV { get; set; }

        ValueChannel ColorA { get; set; }

        Color DefaultColor { get; }

        void UpdateColor(Color color);
    }

    public interface ILayerController : IController
    {
        bool EnableLayerModule { get; set; }

        StringChannel Layer { get; set; }

        ValueChannel Sort { get; set; }

        ValueChannel Alpha { get; set; }

        string DefaultLayer { get; }

        int DefaultSort { get; }

        float DefaultAlpha { get; }

        void UpdateLayer(string layer, int sort, float alpha);
    }

    public interface ITextController : IController
    {
        bool EnableTextModule { get; set; }

        ValueChannel FontSize { get; set; }

        ValueChannel LineSpacing { get; set; }

        TextChannel Text { get; set; }

        string DefaultText { get; }

        float DefaultFontSize { get; }

        float DefaultLineSpacing { get; }

        string DefaultFont { get; }

        string CustomFont { get; set; }

        void UpdateProperties(float fontSize, float lineSpacing);

        void UpdateText(char[] array, int start, int length);

        void ApplyCustomFont(string font);
    }

    public interface INoteGroupController : IController
    {
        bool EnableNoteGroupModule { get; set; }

        ValueChannel AngleX { get; set; }

        ValueChannel AngleY { get; set; }

        ValueChannel RotationIndividualX { get; set; }

        ValueChannel RotationIndividualY { get; set; }

        ValueChannel RotationIndividualZ { get; set; }

        ValueChannel ScaleIndividualX { get; set; }

        ValueChannel ScaleIndividualY { get; set; }

        ValueChannel ScaleIndividualZ { get; set; }

        void UpdateNoteGroup(Quaternion rotation, Vector3 scale, Vector2 angle);
    }

    public interface ICameraController : IController
    {
        bool EnableCameraModule { get; set; }

        ValueChannel FieldOfView { get; set; }

        ValueChannel TiltFactor { get; set; }

        float DefaultFieldOfView { get; }

        void UpdateCamera(float fieldOfView, float tilt);
    }

    public interface IRectController : IController
    {
        bool EnableRectModule { get; set; }

        ValueChannel RectW { get; set; }

        ValueChannel RectH { get; set; }

        ValueChannel AnchorMinX { get; set; }

        ValueChannel AnchorMinY { get; set; }

        ValueChannel AnchorMaxX { get; set; }

        ValueChannel AnchorMaxY { get; set; }

        ValueChannel PivotX { get; set; }

        ValueChannel PivotY { get; set; }

        float DefaultRectW { get; }

        float DefaultRectH { get; }

        Vector2 DefaultAnchorMin { get; }

        Vector2 DefaultAnchorMax { get; }

        Vector2 DefaultPivot { get; }

        void UpdateRect(float w, float h, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot);
    }

    public interface ITextureController : IController
    {
        bool EnableTextureModule { get; set; }

        ValueChannel TextureOffsetX { get; set; }

        ValueChannel TextureOffsetY { get; set; }

        ValueChannel TextureScaleX { get; set; }

        ValueChannel TextureScaleY { get; set; }

        Vector2 DefaultTextureOffset { get; }

        Vector2 DefaultTextureScale { get; }

        void UpdateTexture(Vector2 offset, Vector2 scale);
    }

    public interface ITrackController : IController
    {
        bool EnableTrackModule { get; set; }

        ValueChannel EdgeLAlpha { get; set; }

        ValueChannel EdgeRAlpha { get; set; }

        ValueChannel Lane1Alpha { get; set; }

        ValueChannel Lane2Alpha { get; set; }

        ValueChannel Lane3Alpha { get; set; }

        ValueChannel Lane4Alpha { get; set; }

        string CustomSkin { get; set; }

        void UpdateLane(float edgeL, float edgeR, float lane1, float lane2, float lane3, float lane4);

        void ApplySkin(string customSkin);
    }
}