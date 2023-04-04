using System.Collections.Generic;
using ArcCreate.Utility.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public abstract class Controller : MonoBehaviour, ISerializableUnit, IController, ISceneController
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
            if (this is IPositionController pos && controller is IPositionController pos2)
            {
                pos.TranslationX = pos2.TranslationX;
                pos.TranslationY = pos2.TranslationY;
                pos.TranslationZ = pos2.TranslationZ;
                pos.RotationX = pos2.RotationX;
                pos.RotationY = pos2.RotationY;
                pos.RotationZ = pos2.RotationZ;
                pos.ScaleX = pos2.ScaleX;
                pos.ScaleY = pos2.ScaleY;
                pos.ScaleZ = pos2.ScaleZ;
                pos.EnablePositionModule = pos2.EnablePositionModule;
            }

            if (this is IColorController col && controller is IColorController col2)
            {
                col.ColorR = col2.ColorR;
                col.ColorG = col2.ColorG;
                col.ColorB = col2.ColorB;
                col.ColorA = col2.ColorA;
                col.ColorH = col2.ColorH;
                col.ColorS = col2.ColorS;
                col.ColorV = col2.ColorV;
                col.EnableColorModule = col2.EnableColorModule;
            }

            if (this is ILayerController lyr && controller is ILayerController lyr2)
            {
                lyr.Layer = lyr2.Layer;
                lyr.Sort = lyr2.Sort;
                lyr.Alpha = lyr2.Alpha;
                lyr.EnableLayerModule = lyr2.EnableLayerModule;
            }

            if (this is ITextController txt && controller is ITextController txt2)
            {
                txt.FontSize = txt2.FontSize;
                txt.LineSpacing = txt2.LineSpacing;
                txt.Text = txt2.Text;
                txt.EnableTextModule = txt2.EnableTextModule;
            }

            if (this is INoteGroupController tg && controller is INoteGroupController tg2)
            {
                tg.AngleX = tg2.AngleX;
                tg.AngleY = tg2.AngleY;
                tg.RotationIndividualX = tg2.RotationIndividualX;
                tg.RotationIndividualY = tg2.RotationIndividualY;
                tg.RotationIndividualZ = tg2.RotationIndividualZ;
                tg.ScaleIndividualX = tg2.ScaleIndividualX;
                tg.ScaleIndividualY = tg2.ScaleIndividualY;
                tg.ScaleIndividualZ = tg2.ScaleIndividualZ;
                tg.EnableNoteGroupModule = tg2.EnableNoteGroupModule;
            }

            if (this is ICameraController cam && controller is ICameraController cam2)
            {
                cam.FieldOfView = cam2.FieldOfView;
                cam.TiltFactor = cam2.TiltFactor;
                cam.EnableCameraModule = cam2.EnableCameraModule;
            }

            if (this is IRectController rect && controller is IRectController rect2)
            {
                rect.RectW = rect2.RectW;
                rect.RectH = rect2.RectH;
                rect.AnchorMinX = rect2.AnchorMinX;
                rect.AnchorMinY = rect2.AnchorMinY;
                rect.AnchorMaxX = rect2.AnchorMaxX;
                rect.AnchorMaxY = rect2.AnchorMaxY;
                rect.PivotX = rect2.PivotX;
                rect.PivotY = rect2.PivotY;
                rect.EnableRectModule = rect2.EnableRectModule;
            }

            if (this is ITextureController txtr && controller is ITextureController txtr2)
            {
                txtr.TextureOffsetX = txtr2.TextureOffsetX;
                txtr.TextureOffsetY = txtr2.TextureOffsetY;
                txtr.TextureScaleX = txtr2.TextureScaleX;
                txtr.TextureScaleY = txtr2.TextureScaleY;
                txtr.EnableTextureModule = txtr2.EnableTextureModule;
            }

            if (this is ITrackController track && controller is ITrackController track2)
            {
                track.EdgeLAlpha = track2.EdgeLAlpha;
                track.EdgeRAlpha = track2.EdgeRAlpha;
                track.Lane1Alpha = track2.Lane1Alpha;
                track.Lane2Alpha = track2.Lane2Alpha;
                track.Lane3Alpha = track2.Lane3Alpha;
                track.Lane4Alpha = track2.Lane4Alpha;
                track.EnableTrackModule = track2.EnableTrackModule;
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

            if (this is IPositionController pos && pos.EnablePositionModule)
            {
                Vector3 translation = pos.DefaultTranslation;
                Vector3 rotation = pos.DefaultRotation.eulerAngles;
                Vector3 scale = pos.DefaultScale;

                translation.x = pos.TranslationX.ValueAt(timing);
                translation.y = pos.TranslationY.ValueAt(timing);
                translation.z = pos.TranslationZ.ValueAt(timing);

                rotation.x = pos.RotationX.ValueAt(timing);
                rotation.y = pos.RotationY.ValueAt(timing);
                rotation.z = pos.RotationZ.ValueAt(timing);

                scale.x = pos.ScaleX.ValueAt(timing);
                scale.y = pos.ScaleY.ValueAt(timing);
                scale.z = pos.ScaleZ.ValueAt(timing);

                pos.UpdatePosition(translation, Quaternion.Euler(rotation), scale);
            }

            if (this is IColorController col && col.EnableColorModule)
            {
                RGBA color = new RGBA(col.DefaultColor);
                HSVA modify = new HSVA(0, 0, 0, 1)
                {
                    H = col.ColorH.ValueAt(timing),
                    S = col.ColorS.ValueAt(timing),
                    V = col.ColorV.ValueAt(timing),
                };

                color.R = col.ColorR.ValueAt(timing);
                color.G = col.ColorG.ValueAt(timing);
                color.B = col.ColorB.ValueAt(timing);
                color.A = col.ColorA.ValueAt(timing);

                HSVA hsva = Convert.RGBAToHSVA(color);
                hsva.H = (hsva.H + modify.H) % 360;
                hsva.S = Mathf.Clamp(hsva.S + modify.S, 0, 1);
                hsva.V = Mathf.Clamp(hsva.V + modify.V, 0, 1);

                col.UpdateColor(Convert.HSVAToRGBA(hsva).ToColor());
            }

            if (this is ILayerController lyr && lyr.EnableLayerModule)
            {
                string layer = lyr.DefaultLayer;
                int sort = lyr.DefaultSort;
                float alpha = lyr.DefaultAlpha;

                layer = lyr.Layer.ValueAt(timing);
                sort = (int)lyr.Sort.ValueAt(timing);
                alpha = lyr.Alpha.ValueAt(timing) / 255f;

                lyr.UpdateLayer(layer, sort, alpha);
            }

            if (this is ITextController txt && txt.EnableTextModule)
            {
                float lineSpacing = txt.DefaultLineSpacing;
                float fontSize = txt.DefaultFontSize;

                fontSize = txt.FontSize.ValueAt(timing);
                lineSpacing = txt.LineSpacing.ValueAt(timing);
                char[] text = txt.Text.ValueAt(timing, out int textLength, out bool hasChanged);

                txt.UpdateProperties(fontSize, lineSpacing);
                if (hasChanged)
                {
                    txt.UpdateText(text, 0, textLength);
                }
            }

            if (this is INoteGroupController tg && tg.EnableNoteGroupModule)
            {
                Vector3 rotation = Vector3.zero;
                Vector3 scale = Vector3.one;
                Vector2 angle = Vector2.zero;

                rotation.x = tg.RotationIndividualX.ValueAt(timing);
                rotation.y = tg.RotationIndividualY.ValueAt(timing);
                rotation.z = tg.RotationIndividualZ.ValueAt(timing);

                scale.x = tg.ScaleIndividualX.ValueAt(timing);
                scale.y = tg.ScaleIndividualY.ValueAt(timing);
                scale.z = tg.ScaleIndividualZ.ValueAt(timing);

                angle.x = tg.AngleX.ValueAt(timing);
                angle.y = tg.AngleY.ValueAt(timing);

                tg.UpdateNoteGroup(Quaternion.Euler(rotation), scale, angle);
            }

            if (this is ICameraController cam && cam.EnableCameraModule)
            {
                float fov = cam.DefaultFieldOfView;
                float tilt = 1;

                fov = cam.FieldOfView.ValueAt(timing);
                tilt = cam.TiltFactor.ValueAt(timing);

                cam.UpdateCamera(fov, tilt);
            }

            if (this is IRectController rect && rect.EnableRectModule)
            {
                float rectW = rect.DefaultRectW;
                float rectH = rect.DefaultRectH;
                Vector2 anchorMin = rect.DefaultAnchorMin;
                Vector2 anchorMax = rect.DefaultAnchorMax;
                Vector2 pivot = rect.DefaultPivot;

                rectW = rect.RectW.ValueAt(timing);
                rectH = rect.RectH.ValueAt(timing);
                anchorMin.x = rect.AnchorMinX.ValueAt(timing);
                anchorMin.y = rect.AnchorMinY.ValueAt(timing);
                anchorMax.x = rect.AnchorMaxX.ValueAt(timing);
                anchorMax.y = rect.AnchorMaxY.ValueAt(timing);
                pivot.x = rect.PivotX.ValueAt(timing);
                pivot.y = rect.PivotY.ValueAt(timing);

                rect.UpdateRect(rectW, rectH, anchorMin, anchorMax, pivot);
            }

            if (this is ITextureController txtr && txtr.EnableTextureModule)
            {
                Vector2 offset = txtr.DefaultTextureOffset;
                Vector2 scale = txtr.DefaultTextureScale;

                offset.x = txtr.TextureOffsetX.ValueAt(timing);
                offset.y = txtr.TextureOffsetY.ValueAt(timing);
                scale.x = txtr.TextureScaleX.ValueAt(timing);
                scale.y = txtr.TextureScaleY.ValueAt(timing);

                txtr.UpdateTexture(offset, scale);
            }

            if (this is ITrackController track && track.EnableTrackModule)
            {
                float edgeLAlpha = 1;
                float edgeRAlpha = 1;
                float lane1Alpha = 1;
                float lane2Alpha = 1;
                float lane3Alpha = 1;
                float lane4Alpha = 1;

                edgeLAlpha = track.EdgeLAlpha.ValueAt(timing) / 255f;
                edgeRAlpha = track.EdgeRAlpha.ValueAt(timing) / 255f;
                lane1Alpha = track.Lane1Alpha.ValueAt(timing) / 255f;
                lane2Alpha = track.Lane2Alpha.ValueAt(timing) / 255f;
                lane3Alpha = track.Lane3Alpha.ValueAt(timing) / 255f;
                lane4Alpha = track.Lane4Alpha.ValueAt(timing) / 255f;

                track.UpdateLane(edgeLAlpha, edgeRAlpha, lane1Alpha, lane2Alpha, lane3Alpha, lane4Alpha);
            }
        }

        [MoonSharpHidden]
        public virtual void Reset()
        {
            Active = new ConstantChannel(DefaultActive ? 1 : 0);
            SetActive(DefaultActive);
            if (this is IPositionController pos)
            {
                pos.UpdatePosition(pos.DefaultTranslation, pos.DefaultRotation, pos.DefaultScale);
                pos.TranslationX = new ConstantChannel(pos.DefaultTranslation.x);
                pos.TranslationY = new ConstantChannel(pos.DefaultTranslation.y);
                pos.TranslationZ = new ConstantChannel(pos.DefaultTranslation.z);
                pos.RotationX = new ConstantChannel(pos.DefaultRotation.eulerAngles.x);
                pos.RotationY = new ConstantChannel(pos.DefaultRotation.eulerAngles.y);
                pos.RotationZ = new ConstantChannel(pos.DefaultRotation.eulerAngles.z);
                pos.ScaleX = new ConstantChannel(pos.DefaultScale.x);
                pos.ScaleY = new ConstantChannel(pos.DefaultScale.y);
                pos.ScaleZ = new ConstantChannel(pos.DefaultScale.z);
                pos.EnablePositionModule = false;
            }

            if (this is IColorController col)
            {
                col.UpdateColor(col.DefaultColor);
                col.ColorR = new ConstantChannel(col.DefaultColor.r * 255);
                col.ColorG = new ConstantChannel(col.DefaultColor.g * 255);
                col.ColorB = new ConstantChannel(col.DefaultColor.b * 255);
                col.ColorA = new ConstantChannel(col.DefaultColor.a * 255);
                col.ColorH = new ConstantChannel(0);
                col.ColorS = new ConstantChannel(0);
                col.ColorV = new ConstantChannel(0);
                col.EnableColorModule = false;
            }

            if (this is ILayerController lyr)
            {
                lyr.UpdateLayer(lyr.DefaultLayer, lyr.DefaultSort, lyr.DefaultAlpha);
                lyr.Layer = StringChannelBuilder.Constant(lyr.DefaultLayer);
                lyr.Sort = new ConstantChannel(lyr.DefaultSort);
                lyr.Alpha = new ConstantChannel(lyr.DefaultAlpha * 255f);
                lyr.EnableLayerModule = false;
            }

            if (this is ITextController txt)
            {
                txt.UpdateProperties(txt.DefaultFontSize, txt.DefaultLineSpacing);
                txt.FontSize = new ConstantChannel(txt.DefaultFontSize);
                txt.LineSpacing = new ConstantChannel(txt.DefaultLineSpacing);
                txt.Text = TextChannelBuilder.Constant(txt.DefaultText);
                txt.ApplyCustomFont(txt.DefaultFont);
                txt.CustomFont = null;

                char[] arr = txt.DefaultText.ToCharArray();
                txt.UpdateText(arr, 0, arr.Length);
                txt.EnableTextModule = false;
            }

            if (this is INoteGroupController tg)
            {
                tg.UpdateNoteGroup(Quaternion.identity, Vector3.one, Vector2.zero);
                tg.AngleX = new ConstantChannel(0);
                tg.AngleY = new ConstantChannel(0);
                tg.RotationIndividualX = new ConstantChannel(0);
                tg.RotationIndividualY = new ConstantChannel(0);
                tg.RotationIndividualZ = new ConstantChannel(0);
                tg.ScaleIndividualX = new ConstantChannel(1);
                tg.ScaleIndividualY = new ConstantChannel(1);
                tg.ScaleIndividualZ = new ConstantChannel(1);
                tg.EnableNoteGroupModule = false;
            }

            if (this is ICameraController cam)
            {
                cam.UpdateCamera(cam.DefaultFieldOfView, 1);
                cam.FieldOfView = new ConstantChannel(cam.DefaultFieldOfView);
                cam.TiltFactor = new ConstantChannel(1);
                cam.EnableCameraModule = false;
            }

            if (this is IRectController rect)
            {
                rect.UpdateRect(rect.DefaultRectW, rect.DefaultRectH, rect.DefaultAnchorMin, rect.DefaultAnchorMax, rect.DefaultPivot);
                rect.RectW = new ConstantChannel(rect.DefaultRectW);
                rect.RectH = new ConstantChannel(rect.DefaultRectH);
                rect.AnchorMinX = new ConstantChannel(rect.DefaultAnchorMin.x);
                rect.AnchorMinY = new ConstantChannel(rect.DefaultAnchorMin.y);
                rect.AnchorMaxX = new ConstantChannel(rect.DefaultAnchorMax.x);
                rect.AnchorMaxY = new ConstantChannel(rect.DefaultAnchorMax.y);
                rect.PivotX = new ConstantChannel(rect.DefaultPivot.x);
                rect.PivotY = new ConstantChannel(rect.DefaultPivot.y);
                rect.EnableRectModule = false;
            }

            if (this is ITextureController txtr)
            {
                txtr.UpdateTexture(txtr.DefaultTextureOffset, txtr.DefaultTextureScale);
                txtr.TextureOffsetX = new ConstantChannel(txtr.DefaultTextureOffset.x);
                txtr.TextureOffsetY = new ConstantChannel(txtr.DefaultTextureOffset.y);
                txtr.TextureScaleX = new ConstantChannel(txtr.DefaultTextureScale.x);
                txtr.TextureScaleY = new ConstantChannel(txtr.DefaultTextureScale.y);
                txtr.EnableTextureModule = false;
            }

            if (this is ITrackController track)
            {
                track.UpdateLane(1, 1, 1, 1, 1, 1);
                track.EdgeLAlpha = new ConstantChannel(255);
                track.EdgeRAlpha = new ConstantChannel(255);
                track.Lane1Alpha = new ConstantChannel(255);
                track.Lane2Alpha = new ConstantChannel(255);
                track.Lane3Alpha = new ConstantChannel(255);
                track.Lane4Alpha = new ConstantChannel(255);
                track.CustomSkin = null;
                track.EnableTrackModule = false;
            }
        }

        public List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            List<object> result = new List<object>
            {
                serialization.AddUnitAndGetId(customParent),
                serialization.AddUnitAndGetId(Active),
            };

            if (this is IPositionController pos)
            {
                result.Add(pos.EnablePositionModule);
                result.Add(serialization.AddUnitAndGetId(pos.TranslationX));
                result.Add(serialization.AddUnitAndGetId(pos.TranslationY));
                result.Add(serialization.AddUnitAndGetId(pos.TranslationZ));
                result.Add(serialization.AddUnitAndGetId(pos.RotationX));
                result.Add(serialization.AddUnitAndGetId(pos.RotationY));
                result.Add(serialization.AddUnitAndGetId(pos.RotationZ));
                result.Add(serialization.AddUnitAndGetId(pos.ScaleX));
                result.Add(serialization.AddUnitAndGetId(pos.ScaleY));
                result.Add(serialization.AddUnitAndGetId(pos.ScaleZ));
            }

            if (this is IColorController col)
            {
                result.Add(col.EnableColorModule);
                result.Add(serialization.AddUnitAndGetId(col.ColorR));
                result.Add(serialization.AddUnitAndGetId(col.ColorG));
                result.Add(serialization.AddUnitAndGetId(col.ColorB));
                result.Add(serialization.AddUnitAndGetId(col.ColorA));
                result.Add(serialization.AddUnitAndGetId(col.ColorH));
                result.Add(serialization.AddUnitAndGetId(col.ColorS));
                result.Add(serialization.AddUnitAndGetId(col.ColorV));
            }

            if (this is ILayerController lyr)
            {
                result.Add(lyr.EnableLayerModule);
                result.Add(serialization.AddUnitAndGetId(lyr.Layer));
                result.Add(serialization.AddUnitAndGetId(lyr.Sort));
                result.Add(serialization.AddUnitAndGetId(lyr.Alpha));
            }

            if (this is ITextController txt)
            {
                result.Add(txt.EnableTextModule);
                result.Add(serialization.AddUnitAndGetId(txt.FontSize));
                result.Add(serialization.AddUnitAndGetId(txt.LineSpacing));
                result.Add(serialization.AddUnitAndGetId(txt.Text));
                result.Add(txt.CustomFont);
            }

            if (this is INoteGroupController tg)
            {
                result.Add(tg.EnableNoteGroupModule);
                result.Add(serialization.AddUnitAndGetId(tg.AngleX));
                result.Add(serialization.AddUnitAndGetId(tg.AngleY));
                result.Add(serialization.AddUnitAndGetId(tg.RotationIndividualX));
                result.Add(serialization.AddUnitAndGetId(tg.RotationIndividualY));
                result.Add(serialization.AddUnitAndGetId(tg.RotationIndividualZ));
                result.Add(serialization.AddUnitAndGetId(tg.ScaleIndividualX));
                result.Add(serialization.AddUnitAndGetId(tg.ScaleIndividualY));
                result.Add(serialization.AddUnitAndGetId(tg.ScaleIndividualZ));
            }

            if (this is ICameraController cam)
            {
                result.Add(cam.EnableCameraModule);
                result.Add(serialization.AddUnitAndGetId(cam.FieldOfView));
                result.Add(serialization.AddUnitAndGetId(cam.TiltFactor));
            }

            if (this is IRectController rect)
            {
                result.Add(rect.EnableRectModule);
                result.Add(serialization.AddUnitAndGetId(rect.RectW));
                result.Add(serialization.AddUnitAndGetId(rect.RectH));
                result.Add(serialization.AddUnitAndGetId(rect.AnchorMinX));
                result.Add(serialization.AddUnitAndGetId(rect.AnchorMinY));
                result.Add(serialization.AddUnitAndGetId(rect.AnchorMaxX));
                result.Add(serialization.AddUnitAndGetId(rect.AnchorMaxY));
                result.Add(serialization.AddUnitAndGetId(rect.PivotX));
                result.Add(serialization.AddUnitAndGetId(rect.PivotY));
            }

            if (this is ITextureController txtr)
            {
                result.Add(txtr.EnableTextureModule);
                result.Add(serialization.AddUnitAndGetId(txtr.TextureOffsetX));
                result.Add(serialization.AddUnitAndGetId(txtr.TextureOffsetY));
                result.Add(serialization.AddUnitAndGetId(txtr.TextureScaleX));
                result.Add(serialization.AddUnitAndGetId(txtr.TextureScaleY));
            }

            if (this is ITrackController track)
            {
                result.Add(track.EnableTrackModule);
                result.Add(serialization.AddUnitAndGetId(track.EdgeLAlpha));
                result.Add(serialization.AddUnitAndGetId(track.EdgeRAlpha));
                result.Add(serialization.AddUnitAndGetId(track.Lane1Alpha));
                result.Add(serialization.AddUnitAndGetId(track.Lane2Alpha));
                result.Add(serialization.AddUnitAndGetId(track.Lane3Alpha));
                result.Add(serialization.AddUnitAndGetId(track.Lane4Alpha));
                result.Add(track.CustomSkin);
            }

            return result;
        }

        public void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            customParent = deserialization.GetUnitFromId<Controller>(properties[0]);
            Active = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
            if (customParent != null)
            {
                SetParent(customParent);
            }

            int offset = 2;

            if (this is IPositionController pos)
            {
                bool enable = (bool)properties[offset++];
                pos.TranslationX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.TranslationY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.TranslationZ = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.RotationX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.RotationY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.RotationZ = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.ScaleX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.ScaleY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.ScaleZ = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                pos.EnablePositionModule = enable;
            }

            if (this is IColorController col)
            {
                bool enable = (bool)properties[offset++];
                col.ColorR = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                col.ColorG = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                col.ColorB = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                col.ColorA = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                col.ColorH = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                col.ColorS = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                col.ColorV = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                col.EnableColorModule = enable;
            }

            if (this is ILayerController lyr)
            {
                bool enable = (bool)properties[offset++];
                lyr.Layer = deserialization.GetUnitFromId<StringChannel>(properties[offset++]);
                lyr.Sort = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                lyr.Alpha = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                lyr.EnableLayerModule = enable;
            }

            if (this is ITextController txt)
            {
                bool enable = (bool)properties[offset++];
                txt.FontSize = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                txt.LineSpacing = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                txt.Text = deserialization.GetUnitFromId<TextChannel>(properties[offset++]);
                txt.CustomFont = (string)properties[offset++];
                txt.ApplyCustomFont(txt.CustomFont);
                txt.EnableTextModule = enable;
            }

            if (this is INoteGroupController tg)
            {
                bool enable = (bool)properties[offset++];
                tg.AngleX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                tg.AngleY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                tg.RotationIndividualX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                tg.RotationIndividualY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                tg.RotationIndividualZ = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                tg.ScaleIndividualX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                tg.ScaleIndividualY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                tg.ScaleIndividualZ = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                tg.EnableNoteGroupModule = enable;
            }

            if (this is ICameraController cam)
            {
                bool enable = (bool)properties[offset++];
                cam.FieldOfView = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                cam.TiltFactor = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                cam.EnableCameraModule = enable;
            }

            if (this is IRectController rect)
            {
                bool enable = (bool)properties[offset++];
                rect.RectW = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                rect.RectH = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                rect.AnchorMinX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                rect.AnchorMinY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                rect.AnchorMaxX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                rect.AnchorMaxY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                rect.PivotX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                rect.PivotY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                rect.EnableRectModule = enable;
            }

            if (this is ITextureController txtr)
            {
                bool enable = (bool)properties[offset++];
                txtr.TextureOffsetX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                txtr.TextureOffsetY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                txtr.TextureScaleX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                txtr.TextureScaleY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                txtr.EnableTextureModule = enable;
            }

            if (this is ITrackController track)
            {
                bool enable = (bool)properties[offset++];
                track.EdgeLAlpha = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                track.EdgeRAlpha = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                track.Lane1Alpha = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                track.Lane2Alpha = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                track.Lane3Alpha = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                track.Lane4Alpha = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
                track.CustomSkin = (string)properties[offset++];
                track.ApplySkin(track.CustomSkin);
                track.EnableTrackModule = enable;
            }
        }

        protected virtual void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}