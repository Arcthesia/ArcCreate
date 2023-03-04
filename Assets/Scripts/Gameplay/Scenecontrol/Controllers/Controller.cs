using System.Collections.Generic;
using ArcCreate.Utilities.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class Controller : MonoBehaviour, ISerializableUnit, IController
    {
        [SerializeField] private bool isPersistent = true;
        private Controller customParent;

        private bool defaultActive;

        public ValueChannel Active { get; set; }

        public bool DefaultActive => defaultActive;

        public bool Initialized { get; private set; } = false;

        public string SerializedType { get; set; }

        public XYZ WorldTranslation => new XYZ(transform.position);

        public XYZ WorldRotation => new XYZ(transform.rotation.eulerAngles);

        public XYZ WorldScale => new XYZ(transform.lossyScale);

        public bool IsPersistent
        {
            get => isPersistent;
            set => isPersistent = value;
        }

        [MoonSharpHidden]
        public void Start()
        {
            if (Initialized)
            {
                return;
            }

            Initialized = true;
            defaultActive = gameObject.activeSelf;
            SetupDefault();
            Reset();
        }

        public Controller[] GetChildren()
        {
            return GetComponentsInChildren<Controller>();
        }

        [MoonSharpHidden]
        public void CleanController()
        {
            if (isPersistent)
            {
                Reset();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [MoonSharpHidden]
        public virtual void SetupDefault()
        {
        }

        public void SetParent(Controller controller)
        {
            if (isPersistent)
            {
                throw new System.Exception("Cannot change parent of a persistent controller");
            }

            transform.SetParent(controller.transform);
            customParent = controller;
            if (this is CanvasController canvas)
            {
                canvas.Canvas.overrideSorting = true;
            }
        }

        public void CopyAllChannelsFrom(Controller controller)
        {
            if (this is IPositionController && controller is IPositionController)
            {
                IPositionController c = this as IPositionController;
                IPositionController c2 = controller as IPositionController;
                c.TranslationX = c2.TranslationX;
                c.TranslationY = c2.TranslationY;
                c.TranslationZ = c2.TranslationZ;
                c.RotationX = c2.RotationX;
                c.RotationY = c2.RotationY;
                c.RotationZ = c2.RotationZ;
                c.ScaleX = c2.ScaleX;
                c.ScaleY = c2.ScaleY;
                c.ScaleZ = c2.ScaleZ;
            }

            if (this is IColorController && controller is IColorController)
            {
                IColorController c = this as IColorController;
                IColorController c2 = controller as IColorController;
                c.ColorR = c2.ColorR;
                c.ColorG = c2.ColorG;
                c.ColorB = c2.ColorB;
                c.ColorA = c2.ColorA;
                c.ColorH = c2.ColorH;
                c.ColorS = c2.ColorS;
                c.ColorV = c2.ColorV;
            }

            if (this is ILayerController && controller is ILayerController)
            {
                ILayerController c = this as ILayerController;
                ILayerController c2 = controller as ILayerController;
                c.Layer = c2.Layer;
                c.Sort = c2.Sort;
                c.Alpha = c2.Alpha;
            }

            if (this is ITextController && controller is ITextController)
            {
                ITextController c = this as ITextController;
                ITextController c2 = controller as ITextController;
                c.FontSize = c2.FontSize;
                c.LineSpacing = c2.LineSpacing;
                c.Text = c2.Text;
            }

            if (this is INoteGroupController && controller is INoteGroupController)
            {
                INoteGroupController c = this as INoteGroupController;
                INoteGroupController c2 = controller as INoteGroupController;
                c.AngleX = c2.AngleX;
                c.AngleY = c2.AngleY;
                c.RotationIndividualX = c2.RotationIndividualX;
                c.RotationIndividualY = c2.RotationIndividualY;
                c.RotationIndividualZ = c2.RotationIndividualZ;
                c.ScaleIndividualX = c2.ScaleIndividualX;
                c.ScaleIndividualY = c2.ScaleIndividualY;
                c.ScaleIndividualZ = c2.ScaleIndividualZ;
            }

            if (this is ICameraController && controller is ICameraController)
            {
                ICameraController c = this as ICameraController;
                ICameraController c2 = controller as ICameraController;
                c.FieldOfView = c2.FieldOfView;
                c.TiltFactor = c2.TiltFactor;
            }

            if (this is IRectController && controller is IRectController)
            {
                IRectController c = this as IRectController;
                IRectController c2 = controller as IRectController;
                c.RectW = c2.RectW;
                c.RectH = c2.RectH;
                c.AnchorMinX = c2.AnchorMinX;
                c.AnchorMinY = c2.AnchorMinY;
                c.AnchorMaxX = c2.AnchorMaxX;
                c.AnchorMaxY = c2.AnchorMaxY;
                c.PivotX = c2.PivotX;
                c.PivotY = c2.PivotY;
            }

            if (this is ITextureController && controller is ITextureController)
            {
                ITextureController c = this as ITextureController;
                ITextureController c2 = controller as ITextureController;
                c.TextureOffsetX = c2.TextureOffsetX;
                c.TextureOffsetY = c2.TextureOffsetY;
                c.TextureScaleX = c2.TextureScaleX;
                c.TextureScaleY = c2.TextureScaleY;
            }

            if (this is ITrackController && controller is ITrackController)
            {
                ITrackController c = this as ITrackController;
                ITrackController c2 = controller as ITrackController;
                c.EdgeLAlpha = c2.EdgeLAlpha;
                c.EdgeRAlpha = c2.EdgeRAlpha;
                c.Lane1Alpha = c2.Lane1Alpha;
                c.Lane2Alpha = c2.Lane2Alpha;
                c.Lane3Alpha = c2.Lane3Alpha;
                c.Lane4Alpha = c2.Lane4Alpha;
            }
        }

        [MoonSharpHidden]
        public virtual void UpdateController(int timing)
        {
            SetActive(Active.ValueAt(timing) >= 0.5);
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (this is IPositionController)
            {
                IPositionController c = this as IPositionController;
                Vector3 translation = c.DefaultTranslation;
                Vector3 rotation = c.DefaultRotation.eulerAngles;
                Vector3 scale = c.DefaultScale;

                translation.x = c.TranslationX.ValueAt(timing);
                translation.y = c.TranslationY.ValueAt(timing);
                translation.z = c.TranslationZ.ValueAt(timing);

                rotation.x = c.RotationX.ValueAt(timing);
                rotation.y = c.RotationY.ValueAt(timing);
                rotation.z = c.RotationZ.ValueAt(timing);

                scale.x = c.ScaleX.ValueAt(timing);
                scale.y = c.ScaleY.ValueAt(timing);
                scale.z = c.ScaleZ.ValueAt(timing);

                c.UpdatePosition(translation, Quaternion.Euler(rotation), scale);
            }

            if (this is IColorController)
            {
                IColorController c = this as IColorController;
                RGBA color = new RGBA(c.DefaultColor);
                HSVA modify = new HSVA(0, 0, 0, 1)
                {
                    H = c.ColorH.ValueAt(timing),
                    S = c.ColorS.ValueAt(timing),
                    V = c.ColorV.ValueAt(timing),
                };

                color.R = c.ColorR.ValueAt(timing);
                color.G = c.ColorG.ValueAt(timing);
                color.B = c.ColorB.ValueAt(timing);
                color.A = c.ColorA.ValueAt(timing);

                HSVA hsva = Convert.RGBAToHSVA(color);
                hsva.H = (hsva.H + modify.H) % 360;
                hsva.S = Mathf.Clamp(hsva.S + modify.S, 0, 1);
                hsva.V = Mathf.Clamp(hsva.V + modify.V, 0, 1);

                c.UpdateColor(Convert.HSVAToRGBA(hsva).ToColor());
            }

            if (this is ILayerController)
            {
                ILayerController c = this as ILayerController;
                string layer = c.DefaultLayer;
                int sort = c.DefaultSort;
                float alpha = c.DefaultAlpha;

                layer = c.Layer.ValueAt(timing);
                sort = (int)c.Sort.ValueAt(timing);
                alpha = c.Alpha.ValueAt(timing) / 255f;

                c.UpdateLayer(layer, sort, alpha);
            }

            if (this is ITextController)
            {
                ITextController c = this as ITextController;
                float lineSpacing = c.DefaultLineSpacing;
                float fontSize = c.DefaultFontSize;

                fontSize = c.FontSize.ValueAt(timing);
                lineSpacing = c.LineSpacing.ValueAt(timing);
                char[] text = c.Text.ValueAt(timing, out int textLength);

                c.UpdateProperties(fontSize, lineSpacing);
                c.UpdateText(text, 0, textLength);
            }

            if (this is INoteGroupController)
            {
                INoteGroupController c = this as INoteGroupController;
                Vector3 rotation = Vector3.zero;
                Vector3 scale = Vector3.one;
                Vector2 angle = Vector2.zero;

                rotation.x = c.RotationIndividualX.ValueAt(timing);
                rotation.y = c.RotationIndividualY.ValueAt(timing);
                rotation.z = c.RotationIndividualZ.ValueAt(timing);

                scale.x = c.ScaleIndividualX.ValueAt(timing);
                scale.y = c.ScaleIndividualY.ValueAt(timing);
                scale.z = c.ScaleIndividualZ.ValueAt(timing);

                angle.x = c.AngleX.ValueAt(timing);
                angle.y = c.AngleY.ValueAt(timing);

                c.UpdateNoteGroup(Quaternion.Euler(rotation), scale, angle);
            }

            if (this is ICameraController)
            {
                ICameraController c = this as ICameraController;
                float fov = c.DefaultFieldOfView;
                float tilt = 1;

                fov = c.FieldOfView.ValueAt(timing);
                tilt = c.TiltFactor.ValueAt(timing);

                c.UpdateCamera(fov, tilt);
            }

            if (this is IRectController)
            {
                IRectController c = this as IRectController;
                float rectW = c.DefaultRectW;
                float rectH = c.DefaultRectH;
                Vector2 anchorMin = c.DefaultAnchorMin;
                Vector2 anchorMax = c.DefaultAnchorMax;
                Vector2 pivot = c.DefaultPivot;

                rectW = c.RectW.ValueAt(timing);
                rectH = c.RectH.ValueAt(timing);
                anchorMin.x = c.AnchorMinX.ValueAt(timing);
                anchorMin.y = c.AnchorMinY.ValueAt(timing);
                anchorMax.x = c.AnchorMaxX.ValueAt(timing);
                anchorMax.y = c.AnchorMaxY.ValueAt(timing);
                pivot.x = c.PivotX.ValueAt(timing);
                pivot.y = c.PivotY.ValueAt(timing);

                c.UpdateRect(rectW, rectH, anchorMin, anchorMax, pivot);
            }

            if (this is ITextureController)
            {
                ITextureController c = this as ITextureController;
                Vector2 offset = c.DefaultTextureOffset;
                Vector2 scale = c.DefaultTextureScale;

                offset.x = c.TextureOffsetX.ValueAt(timing);
                offset.y = c.TextureOffsetY.ValueAt(timing);
                scale.x = c.TextureScaleX.ValueAt(timing);
                scale.y = c.TextureScaleY.ValueAt(timing);

                c.UpdateTexture(offset, scale);
            }

            if (this is ITrackController)
            {
                ITrackController c = this as ITrackController;

                float edgeLAlpha = 1;
                float edgeRAlpha = 1;
                float lane1Alpha = 1;
                float lane2Alpha = 1;
                float lane3Alpha = 1;
                float lane4Alpha = 1;

                edgeLAlpha = c.EdgeLAlpha.ValueAt(timing) / 255f;
                edgeRAlpha = c.EdgeRAlpha.ValueAt(timing) / 255f;
                lane1Alpha = c.Lane1Alpha.ValueAt(timing) / 255f;
                lane2Alpha = c.Lane2Alpha.ValueAt(timing) / 255f;
                lane3Alpha = c.Lane3Alpha.ValueAt(timing) / 255f;
                lane4Alpha = c.Lane4Alpha.ValueAt(timing) / 255f;

                c.UpdateLane(edgeLAlpha, edgeRAlpha, lane1Alpha, lane2Alpha, lane3Alpha, lane4Alpha);
            }
        }

        [MoonSharpHidden]
        public virtual void Reset()
        {
            Active = new ConstantChannel(DefaultActive ? 1 : 0);
            SetActive(DefaultActive);
            if (this is IPositionController)
            {
                IPositionController c = this as IPositionController;
                c.UpdatePosition(c.DefaultTranslation, c.DefaultRotation, c.DefaultScale);
                c.TranslationX = new ConstantChannel(c.DefaultTranslation.x);
                c.TranslationY = new ConstantChannel(c.DefaultTranslation.y);
                c.TranslationZ = new ConstantChannel(c.DefaultTranslation.z);
                c.RotationX = new ConstantChannel(c.DefaultRotation.eulerAngles.x);
                c.RotationY = new ConstantChannel(c.DefaultRotation.eulerAngles.y);
                c.RotationZ = new ConstantChannel(c.DefaultRotation.eulerAngles.z);
                c.ScaleX = new ConstantChannel(c.DefaultScale.x);
                c.ScaleY = new ConstantChannel(c.DefaultScale.y);
                c.ScaleZ = new ConstantChannel(c.DefaultScale.z);
            }

            if (this is IColorController)
            {
                IColorController c = this as IColorController;
                c.UpdateColor(c.DefaultColor);
                c.ColorR = new ConstantChannel(c.DefaultColor.r * 255);
                c.ColorG = new ConstantChannel(c.DefaultColor.g * 255);
                c.ColorB = new ConstantChannel(c.DefaultColor.b * 255);
                c.ColorA = new ConstantChannel(c.DefaultColor.a * 255);
                c.ColorH = new ConstantChannel(0);
                c.ColorS = new ConstantChannel(0);
                c.ColorV = new ConstantChannel(0);
            }

            if (this is ILayerController)
            {
                ILayerController c = this as ILayerController;
                c.UpdateLayer(c.DefaultLayer, c.DefaultSort, c.DefaultAlpha);
                c.Layer = StringChannelBuilder.Constant(c.DefaultLayer);
                c.Sort = new ConstantChannel(c.DefaultSort);
                c.Alpha = new ConstantChannel(c.DefaultAlpha * 255f);
            }

            if (this is ITextController)
            {
                ITextController c = this as ITextController;
                c.UpdateProperties(c.DefaultFontSize, c.DefaultLineSpacing);
                c.FontSize = new ConstantChannel(c.DefaultFontSize);
                c.LineSpacing = new ConstantChannel(c.DefaultLineSpacing);
                c.Text = TextChannelBuilder.Constant(c.DefaultText);

                char[] arr = c.DefaultText.ToCharArray();
                c.UpdateText(arr, 0, arr.Length);
            }

            if (this is INoteGroupController)
            {
                INoteGroupController c = this as INoteGroupController;
                c.UpdateNoteGroup(Quaternion.identity, Vector3.one, Vector2.zero);
                c.AngleX = new ConstantChannel(0);
                c.AngleY = new ConstantChannel(0);
                c.RotationIndividualX = new ConstantChannel(0);
                c.RotationIndividualY = new ConstantChannel(0);
                c.RotationIndividualZ = new ConstantChannel(0);
                c.ScaleIndividualX = new ConstantChannel(1);
                c.ScaleIndividualY = new ConstantChannel(1);
                c.ScaleIndividualZ = new ConstantChannel(1);
            }

            if (this is ICameraController)
            {
                ICameraController c = this as ICameraController;
                c.UpdateCamera(c.DefaultFieldOfView, 1);
                c.FieldOfView = new ConstantChannel(c.DefaultFieldOfView);
                c.TiltFactor = new ConstantChannel(1);
            }

            if (this is IRectController)
            {
                IRectController c = this as IRectController;
                c.UpdateRect(c.DefaultRectW, c.DefaultRectH, c.DefaultAnchorMin, c.DefaultAnchorMax, c.DefaultPivot);
                c.RectW = new ConstantChannel(c.DefaultRectW);
                c.RectH = new ConstantChannel(c.DefaultRectH);
                c.AnchorMinX = new ConstantChannel(c.DefaultAnchorMin.x);
                c.AnchorMinY = new ConstantChannel(c.DefaultAnchorMin.y);
                c.AnchorMaxX = new ConstantChannel(c.DefaultAnchorMax.x);
                c.AnchorMaxY = new ConstantChannel(c.DefaultAnchorMax.y);
                c.PivotX = new ConstantChannel(c.DefaultPivot.x);
                c.PivotY = new ConstantChannel(c.DefaultPivot.y);
            }

            if (this is ITextureController)
            {
                ITextureController c = this as ITextureController;
                c.UpdateTexture(c.DefaultTextureOffset, c.DefaultTextureScale);
                c.TextureOffsetX = new ConstantChannel(c.DefaultTextureOffset.x);
                c.TextureOffsetY = new ConstantChannel(c.DefaultTextureOffset.y);
                c.TextureScaleX = new ConstantChannel(c.DefaultTextureScale.x);
                c.TextureScaleY = new ConstantChannel(c.DefaultTextureScale.y);
            }

            if (this is ITrackController)
            {
                ITrackController c = this as ITrackController;
                c.UpdateLane(1, 1, 1, 1, 1, 1);
                c.EdgeLAlpha = new ConstantChannel(255);
                c.EdgeRAlpha = new ConstantChannel(255);
                c.Lane1Alpha = new ConstantChannel(255);
                c.Lane2Alpha = new ConstantChannel(255);
                c.Lane3Alpha = new ConstantChannel(255);
                c.Lane4Alpha = new ConstantChannel(255);
            }
        }

        void ISerializableUnit.Destroy()
        {
            CleanController();
        }

        void ISerializableUnit.Reset()
        {
        }

        public List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            List<object> result = new List<object>
            {
                serialization.AddUnitAndGetId(customParent),
            };

            if (this is IPositionController)
            {
                IPositionController c = this as IPositionController;
                result.Add(serialization.AddUnitAndGetId(c.TranslationX));
                result.Add(serialization.AddUnitAndGetId(c.TranslationY));
                result.Add(serialization.AddUnitAndGetId(c.TranslationZ));
                result.Add(serialization.AddUnitAndGetId(c.RotationX));
                result.Add(serialization.AddUnitAndGetId(c.RotationY));
                result.Add(serialization.AddUnitAndGetId(c.RotationZ));
                result.Add(serialization.AddUnitAndGetId(c.ScaleX));
                result.Add(serialization.AddUnitAndGetId(c.ScaleY));
                result.Add(serialization.AddUnitAndGetId(c.ScaleZ));
            }

            if (this is IColorController)
            {
                IColorController c = this as IColorController;
                result.Add(serialization.AddUnitAndGetId(c.ColorR));
                result.Add(serialization.AddUnitAndGetId(c.ColorG));
                result.Add(serialization.AddUnitAndGetId(c.ColorB));
                result.Add(serialization.AddUnitAndGetId(c.ColorA));
                result.Add(serialization.AddUnitAndGetId(c.ColorH));
                result.Add(serialization.AddUnitAndGetId(c.ColorS));
                result.Add(serialization.AddUnitAndGetId(c.ColorV));
            }

            if (this is ILayerController)
            {
                ILayerController c = this as ILayerController;
                result.Add(serialization.AddUnitAndGetId(c.Layer));
                result.Add(serialization.AddUnitAndGetId(c.Sort));
                result.Add(serialization.AddUnitAndGetId(c.Alpha));
            }

            if (this is ITextController)
            {
                ITextController c = this as ITextController;
                result.Add(serialization.AddUnitAndGetId(c.FontSize));
                result.Add(serialization.AddUnitAndGetId(c.LineSpacing));
                result.Add(serialization.AddUnitAndGetId(c.Text));
            }

            if (this is INoteGroupController)
            {
                INoteGroupController c = this as INoteGroupController;
                result.Add(serialization.AddUnitAndGetId(c.AngleX));
                result.Add(serialization.AddUnitAndGetId(c.AngleY));
                result.Add(serialization.AddUnitAndGetId(c.RotationIndividualX));
                result.Add(serialization.AddUnitAndGetId(c.RotationIndividualY));
                result.Add(serialization.AddUnitAndGetId(c.RotationIndividualZ));
                result.Add(serialization.AddUnitAndGetId(c.ScaleIndividualX));
                result.Add(serialization.AddUnitAndGetId(c.ScaleIndividualY));
                result.Add(serialization.AddUnitAndGetId(c.ScaleIndividualZ));
            }

            if (this is ICameraController)
            {
                ICameraController c = this as ICameraController;
                result.Add(serialization.AddUnitAndGetId(c.FieldOfView));
                result.Add(serialization.AddUnitAndGetId(c.TiltFactor));
            }

            if (this is IRectController)
            {
                IRectController c = this as IRectController;
                result.Add(serialization.AddUnitAndGetId(c.RectW));
                result.Add(serialization.AddUnitAndGetId(c.RectH));
                result.Add(serialization.AddUnitAndGetId(c.AnchorMinX));
                result.Add(serialization.AddUnitAndGetId(c.AnchorMinY));
                result.Add(serialization.AddUnitAndGetId(c.AnchorMaxX));
                result.Add(serialization.AddUnitAndGetId(c.AnchorMaxY));
                result.Add(serialization.AddUnitAndGetId(c.PivotX));
                result.Add(serialization.AddUnitAndGetId(c.PivotY));
            }

            if (this is ITextureController)
            {
                ITextureController c = this as ITextureController;
                result.Add(serialization.AddUnitAndGetId(c.TextureOffsetX));
                result.Add(serialization.AddUnitAndGetId(c.TextureOffsetY));
                result.Add(serialization.AddUnitAndGetId(c.TextureScaleX));
                result.Add(serialization.AddUnitAndGetId(c.TextureScaleY));
            }

            if (this is ITrackController)
            {
                ITrackController c = this as ITrackController;
                result.Add(serialization.AddUnitAndGetId(c.EdgeLAlpha));
                result.Add(serialization.AddUnitAndGetId(c.EdgeRAlpha));
                result.Add(serialization.AddUnitAndGetId(c.Lane1Alpha));
                result.Add(serialization.AddUnitAndGetId(c.Lane2Alpha));
                result.Add(serialization.AddUnitAndGetId(c.Lane3Alpha));
                result.Add(serialization.AddUnitAndGetId(c.Lane4Alpha));
            }

            return result;
        }

        public void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            customParent = deserialization.GetUnitFromId((int)properties[0]) as Controller;
            if (customParent != null)
            {
                SetParent(customParent);
            }

            int offset = 1;

            if (this is IPositionController)
            {
                IPositionController c = this as IPositionController;
                c.TranslationX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.TranslationY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.TranslationZ = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.RotationX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.RotationY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.RotationZ = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ScaleX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ScaleY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ScaleZ = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
            }

            if (this is IColorController)
            {
                IColorController c = this as IColorController;
                c.ColorR = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ColorG = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ColorB = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ColorA = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ColorH = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ColorS = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ColorV = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
            }

            if (this is ILayerController)
            {
                ILayerController c = this as ILayerController;
                c.Layer = deserialization.GetUnitFromId((int)properties[offset++]) as StringChannel;
                c.Sort = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.Alpha = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
            }

            if (this is ITextController)
            {
                ITextController c = this as ITextController;
                c.FontSize = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.LineSpacing = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.Text = deserialization.GetUnitFromId((int)properties[offset++]) as TextChannel;
            }

            if (this is INoteGroupController)
            {
                INoteGroupController c = this as INoteGroupController;
                c.AngleX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.AngleY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.RotationIndividualX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.RotationIndividualY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.RotationIndividualZ = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ScaleIndividualX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ScaleIndividualY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.ScaleIndividualZ = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
            }

            if (this is ICameraController)
            {
                ICameraController c = this as ICameraController;
                c.FieldOfView = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.TiltFactor = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
            }

            if (this is IRectController)
            {
                IRectController c = this as IRectController;
                c.RectW = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.RectH = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.AnchorMinX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.AnchorMinY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.AnchorMaxX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.AnchorMaxY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.PivotX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.PivotY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
            }

            if (this is ITextureController)
            {
                ITextureController c = this as ITextureController;
                c.TextureOffsetX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.TextureOffsetY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.TextureScaleX = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.TextureScaleY = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
            }

            if (this is ITrackController)
            {
                ITrackController c = this as ITrackController;
                c.EdgeLAlpha = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.EdgeRAlpha = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.Lane1Alpha = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.Lane2Alpha = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.Lane3Alpha = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
                c.Lane4Alpha = deserialization.GetUnitFromId((int)properties[offset++]) as ValueChannel;
            }
        }

        protected virtual void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}