using System.IO;
using ArcCreate.Utility.Parser;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class Scene : MonoBehaviour
    {
#pragma warning disable
        [Header("Internal")]
        public CameraController GameplayCamera;
        public TextController Combo;
        public TextController Score;
        public ImageController Jacket;
        public TitleController Title;
        public ComposerController Composer;
        public DifficultyController DifficultyText;
        public ImageController DifficultyBackground;
        public CanvasController HUD;
        public CanvasController hud => HUD;
        public InfoPanelController InfoPanel;
        public ImageController PauseButton;
        public ImageController Background;
        public SpriteController VideoBackground;
        public TrackController Track;
        public SingleLineController SingleLineL;
        public SingleLineController SingleLineR;
        public GlowingSpriteController SkyInputLine;
        public GlowingSpriteController SkyInputLabel;
        public BeatlinesController Beatlines;
        public SpriteController Darken;

        [Header("Prefab")]
        public GameObject ImagePrefab;
        public GameObject CanvasPrefab;
        public GameObject SpritePrefab;
        public GameObject TextPrefab;
        public GameObject GroupPrefab;

        [Header("Canvases")]
        public CanvasController WorldCanvas;
        public CanvasController ScreenCanvas;
        public CanvasController CameraCanvas;
        public Transform CanvasParent;

        [Header("Materials")]
        public Material DefaultMaterial;
        public Material ColorBurnMaterial;
        public Material ColorDodgeMaterial;
        public Material DarkenMaterial;
        public Material DifferenceMaterial;
        public Material ExclusionMaterial;
        public Material FastAddMaterial;
        public Material FastDarkenMaterial;
        public Material FastLightenMaterial;
        public Material FastMultiplyMaterial;
        public Material FastScreenMaterial;
        public Material HardLightMaterial;
        public Material LightenMaterial;
        public Material LinearBurnMaterial;
        public Material LinearDodgeMaterial;
        public Material LinearLightMaterial;
        public Material MultiplyMaterial;
        public Material OverlayMaterial;
        public Material ScreenMaterial;
        public Material SoftLightMaterial;
        public Material SubtractMaterial;
        public Material VividLightMaterial;
        public Material MeshMaterial;

        [MoonSharpHidden] public Controller[] DisabledByDefault;
#pragma warning restore

        public Material GetMaterial(string material, bool newMaterialInstance)
        {
            Material m = GetMaterial(material);
            if (newMaterialInstance)
            {
                return Instantiate(m);
            }
            else
            {
                return m;
            }
        }

        public ImageController CreateImage(string imgPath, string material = "default")
        {
            GameObject obj = Instantiate(ImagePrefab, ScreenCanvas.transform);
            ImageController c = obj.GetComponent<ImageController>();
            c.Image.material = GetMaterial(material);

            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(File.ReadAllBytes(Path.Combine(Services.Scenecontrol.SceneControlFolder, imgPath)));
            c.Image.texture = t;

            c.Start();
            c.SerializedType = $"image.{imgPath},{material}";
            return c;
        }

        public SpriteController CreateSprite(string imgPath, string material = "default")
        {
            GameObject obj = Instantiate(SpritePrefab, ScreenCanvas.transform);
            SpriteController c = obj.GetComponent<SpriteController>();
            c.SpriteRenderer.material = GetMaterial(material);

            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(File.ReadAllBytes(Path.Combine(Services.Scenecontrol.SceneControlFolder, imgPath)));
            c.SpriteRenderer.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100);

            c.Start();
            c.SerializedType = $"sprite.{imgPath},{material}";
            return c;
        }

        public CanvasController CreateCanvas(bool worldSpace = false)
        {
            GameObject obj = Instantiate(CanvasPrefab, CanvasParent);
            CanvasController c = obj.GetComponent<CanvasController>();
            c.Canvas.overrideSorting = true;
            if (worldSpace)
            {
                c.Canvas.renderMode = RenderMode.WorldSpace;
                c.Canvas.transform.localPosition = Vector3.zero;
                c.Canvas.transform.localRotation = Quaternion.identity;
                c.Canvas.transform.localScale = Vector3.one;
            }
            else
            {
                c.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
                c.Canvas.worldCamera = Services.Camera.GameplayCamera;
            }

            c.Start();
            c.SerializedType = $"canvas.{worldSpace}";
            return c;
        }

        public TextController CreateText(
            string font = "default",
            float fontSize = 40,
            float lineSpacing = 1,
            string alignment = "middlecenter")
        {
            GameObject obj = Instantiate(TextPrefab, ScreenCanvas.transform);
            TextController c = obj.GetComponent<TextController>();
            c.SetFont(font);
            c.TextComponent.fontSize = Mathf.RoundToInt(fontSize);
            c.TextComponent.lineSpacing = lineSpacing;
            switch (alignment.ToLower())
            {
                case "upperleft":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.TopLeft;
                    break;
                case "uppercenter":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.Top;
                    break;
                case "upperright":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.TopRight;
                    break;
                case "middleleft":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.Left;
                    break;
                case "middlecenter":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.Center;
                    break;
                case "middleright":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.Right;
                    break;
                case "lowerleft":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                    break;
                case "lowercenter":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.Bottom;
                    break;
                case "lowerright":
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.BottomRight;
                    break;
                default:
                    c.TextComponent.alignment = TMPro.TextAlignmentOptions.Center;
                    break;
            }

            c.SerializedType = $"text.{font},{fontSize},{lineSpacing},{alignment}";
            return c;
        }

        public NoteGroupController GetNoteGroup(int tg)
        {
            try
            {
                var group = Services.Chart.GetTimingGroup(tg);
                NoteGroupController contr = Instantiate(GroupPrefab, transform).GetComponent<NoteGroupController>();
                contr.TimingGroup = group;
                contr.SerializedType = $"tg.{group.GroupNumber}";
                return contr;
            }
            catch
            {
                return null;
            }
        }

        public Controller CreateFromTypeName(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            bool copy = type[0] == '$';
            string def = string.Empty;
            string arg = string.Empty;
            StringParser parser = new StringParser(type);
            if (copy)
            {
                parser.Skip(1);
            }

            if (type.Contains("."))
            {
                def = parser.ReadString(".");
                arg = parser.ReadString();
            }
            else
            {
                def = type;
            }

            Controller c = GetBaseControlerFromTypeName(def, arg);
            if (copy && c.IsPersistent)
            {
                switch (c)
                {
                    case CanvasController canvas:
                        return canvas.Copy();
                    case ImageController image:
                        return image.Copy();
                    case SpriteController sprite:
                        return sprite.Copy();
                    case TextController text:
                        return text.Copy();
                }
            }

            return c;
        }

        private Controller GetBaseControlerFromTypeName(string def, string arg)
        {
            switch (def)
            {
                case "camera":
                    return GameplayCamera;
                case "combo":
                    return Combo;
                case "score":
                    return Score;
                case "jacket":
                    return Jacket;
                case "title":
                    return Title;
                case "composer":
                    return Composer;
                case "diff":
                    return DifficultyText;
                case "diffBg":
                    return DifficultyBackground;
                case "hud":
                    return HUD;
                case "info":
                    return InfoPanel;
                case "pause":
                    return PauseButton;
                case "bg":
                    return Background;
                case "videobg":
                    return VideoBackground;
                case "track":
                    return Track;
                case "singlelinel":
                    return SingleLineL;
                case "singleliner":
                    return SingleLineR;
                case "skyinputline":
                    return SkyInputLine;
                case "skyinputlabel":
                    return SkyInputLabel;
                case "beatlines":
                    return Beatlines;
                case "darken":
                    return Darken;
                case "image":
                    string[] imgsplit = def.Split(',');
                    return CreateImage(imgsplit[0], imgsplit[1]);
                case "canvas":
                    bool worldSpace = bool.Parse(def);
                    return CreateCanvas(worldSpace);
                case "sprite":
                    string[] spriteSplit = def.Split(',');
                    return CreateSprite(spriteSplit[0], spriteSplit[1]);
                case "text":
                    string[] textSplit = def.Split(',');
                    return CreateText(
                        textSplit[0],
                        float.Parse(textSplit[1]),
                        float.Parse(textSplit[2]),
                        textSplit[3]);
                case "worldcanvas":
                    return WorldCanvas;
                case "screencanvas":
                    return ScreenCanvas;
                case "cameracanvas":
                    return CameraCanvas;
                case "tg":
                    return GetNoteGroup(int.Parse(arg));
                default:
                    return null;
            }
        }

        private Material GetMaterial(string material)
        {
            switch (material.ToLower())
            {
                case "default":
                    return DefaultMaterial;
                case "colorburn":
                    return ColorBurnMaterial;
                case "colordodge":
                    return ColorDodgeMaterial;
                case "darken":
                    return DarkenMaterial;
                case "difference":
                    return DifferenceMaterial;
                case "exclusion":
                    return ExclusionMaterial;
                case "add":
                case "fastadd":
                    return FastAddMaterial;
                case "fastdarken":
                    return FastDarkenMaterial;
                case "fastlighten":
                    return FastLightenMaterial;
                case "fastmultiply":
                    return FastMultiplyMaterial;
                case "fastscreen":
                    return FastScreenMaterial;
                case "hardlight":
                    return HardLightMaterial;
                case "lighten":
                    return LightenMaterial;
                case "linearburn":
                    return LinearBurnMaterial;
                case "lineardodge":
                    return LinearDodgeMaterial;
                case "linearlight":
                    return LinearLightMaterial;
                case "multiply":
                    return MultiplyMaterial;
                case "overlay":
                    return OverlayMaterial;
                case "screen":
                    return ScreenMaterial;
                case "softlight":
                    return SoftLightMaterial;
                case "subtract":
                    return SubtractMaterial;
                case "vividlight":
                    return VividLightMaterial;
                default:
                    return DefaultMaterial;
            }
        }

        private void Awake()
        {
            GameplayCamera.SerializedType = "camera";
            Combo.SerializedType = "combo";
            Score.SerializedType = "score";
            Jacket.SerializedType = "jacket";
            Title.SerializedType = "title";
            Composer.SerializedType = "composer";
            DifficultyText.SerializedType = "diff";
            DifficultyBackground.SerializedType = "diffBg";
            HUD.SerializedType = "hud";
            InfoPanel.SerializedType = "info";
            PauseButton.SerializedType = "pause";
            Background.SerializedType = "bg";
            VideoBackground.SerializedType = "videobg";
            Track.SerializedType = "track";
            SingleLineL.SerializedType = "singlelinel";
            SingleLineR.SerializedType = "singleliner";
            SkyInputLine.SerializedType = "skyinputline";
            SkyInputLabel.SerializedType = "skyinputlabel";
            Beatlines.SerializedType = "beatlines";
            Darken.SerializedType = "darken";
            WorldCanvas.SerializedType = "worldcanvas";
            ScreenCanvas.SerializedType = "screencanvas";
            CameraCanvas.SerializedType = "cameracanvas";
        }
    }
}