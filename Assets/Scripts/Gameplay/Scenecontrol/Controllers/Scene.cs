using System.Collections.Generic;
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
        [SerializeField] private CameraController gameplayCamera;
        public CameraController GameplayCamera
        {
            get
            {
                ReferencedControllers.Add(gameplayCamera);
                return gameplayCamera;
            }
        }
        [SerializeField] private TextController combo;
        public TextController Combo
        {
            get
            {
                ReferencedControllers.Add(combo);
                return combo;
            }
        }
        [SerializeField] private TextController score;
        public TextController Score
        {
            get
            {
                ReferencedControllers.Add(score);
                return score;
            }
        }
        [SerializeField] private ImageController jacket;
        public ImageController Jacket
        {
            get
            {
                ReferencedControllers.Add(jacket);
                return jacket;
            }
        }
        [SerializeField] private TitleController title;
        public TitleController Title
        {
            get
            {
                ReferencedControllers.Add(title);
                return title;
            }
        }
        [SerializeField] private ComposerController composer;
        public ComposerController Composer
        {
            get
            {
                ReferencedControllers.Add(composer);
                return composer;
            }
        }
        [SerializeField] private DifficultyController difficultyText;
        public DifficultyController DifficultyText
        {
            get
            {
                ReferencedControllers.Add(difficultyText);
                return difficultyText;
            }
        }
        [SerializeField] private ImageController difficultyBackground;
        public ImageController DifficultyBackground
        {
            get
            {
                ReferencedControllers.Add(difficultyBackground);
                return difficultyBackground;
            }
        }
        [SerializeField] private CanvasController hUD;
        public CanvasController HUD
        {
            get
            {
                ReferencedControllers.Add(hUD);
                return hUD;
            }
        }
        public CanvasController hud => HUD;
        [SerializeField] private InfoPanelController infoPanel;
        public InfoPanelController InfoPanel
        {
            get
            {
                ReferencedControllers.Add(infoPanel);
                return infoPanel;
            }
        }
        [SerializeField] private ImageController pauseButton;
        public ImageController PauseButton
        {
            get
            {
                ReferencedControllers.Add(pauseButton);
                return pauseButton;
            }
        }
        [SerializeField] private ImageController background;
        public ImageController Background
        {
            get
            {
                ReferencedControllers.Add(background);
                return background;
            }
        }
        [SerializeField] private SpriteController videoBackground;
        public SpriteController VideoBackground
        {
            get
            {
                ReferencedControllers.Add(videoBackground);
                return videoBackground;
            }
        }
        [SerializeField] private TrackController track;
        public TrackController Track
        {
            get
            {
                ReferencedControllers.Add(track);
                return track;
            }
        }
        [SerializeField] private SingleLineController singleLineL;
        public SingleLineController SingleLineL
        {
            get
            {
                ReferencedControllers.Add(singleLineL);
                return singleLineL;
            }
        }
        [SerializeField] private SingleLineController singleLineR;
        public SingleLineController SingleLineR
        {
            get
            {
                ReferencedControllers.Add(singleLineR);
                return singleLineR;
            }
        }
        [SerializeField] private GlowingSpriteController skyInputLine;
        public GlowingSpriteController SkyInputLine
        {
            get
            {
                ReferencedControllers.Add(skyInputLine);
                return skyInputLine;
            }
        }
        [SerializeField] private GlowingSpriteController skyInputLabel;
        public GlowingSpriteController SkyInputLabel
        {
            get
            {
                ReferencedControllers.Add(skyInputLabel);
                return skyInputLabel;
            }
        }
        [SerializeField] private BeatlinesController beatlines;
        public BeatlinesController Beatlines
        {
            get
            {
                ReferencedControllers.Add(beatlines);
                return beatlines;
            }
        }
        [SerializeField] private SpriteController darken;
        public SpriteController Darken
        {
            get
            {
                ReferencedControllers.Add(darken);
                return darken;
            }
        }
        [SerializeField] private CanvasController worldCanvas;
        public CanvasController WorldCanvas
        {
            get
            {
                ReferencedControllers.Add(worldCanvas);
                return worldCanvas;
            }
        }
        [SerializeField] private CanvasController screenCanvas;
        public CanvasController ScreenCanvas
        {
            get
            {
                ReferencedControllers.Add(screenCanvas);
                return screenCanvas;
            }
        }
        [SerializeField] private CanvasController cameraCanvas;
        public CanvasController CameraCanvas
        {
            get
            {
                ReferencedControllers.Add(cameraCanvas);
                return cameraCanvas;
            }
        }

        public Transform CanvasParent;

        [Header("Prefab")]
        public GameObject ImagePrefab;
        public GameObject CanvasPrefab;
        public GameObject SpritePrefab;
        public GameObject TextPrefab;
        public GameObject GroupPrefab;

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

        [SerializeField] private int overlayLayer;
        [SerializeField] private int notesLayer;
        [SerializeField] private int backgroundLayer;

        [MoonSharpHidden] public List<Controller> DisabledByDefault;

        private List<Controller> ReferencedControllers => Services.Scenecontrol.ReferencedControllers;
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

        public ImageController CreateImage(string imgPath, string material = "default", string renderLayer = "overlay")
        {
            GameObject obj = Instantiate(ImagePrefab, ScreenCanvas.transform);
            obj.layer = GetLayer(renderLayer);
            ImageController c = obj.GetComponent<ImageController>();
            c.Image.material = GetMaterial(material);

            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(File.ReadAllBytes(Path.Combine(Services.Scenecontrol.ScenecontrolFolder, imgPath)));
            c.Image.sprite = Sprite.Create(
                texture: t,
                rect: new Rect(0, 0, t.width, t.height),
                pivot: new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 100,
                extrude: 1,
                meshType: SpriteMeshType.FullRect);

            c.Start();
            c.SerializedType = $"image.{imgPath},{material},{renderLayer}";
            ReferencedControllers.Add(c);
            return c;
        }

        public SpriteController CreateSprite(string imgPath, string material = "default", string renderLayer = "overlay")
        {
            GameObject obj = Instantiate(SpritePrefab, ScreenCanvas.transform);
            obj.layer = GetLayer(renderLayer);
            SpriteController c = obj.GetComponent<SpriteController>();
            c.SpriteRenderer.material = GetMaterial(material);

            Texture2D t = new Texture2D(1, 1);
            t.LoadImage(File.ReadAllBytes(Path.Combine(Services.Scenecontrol.ScenecontrolFolder, imgPath)));
            c.SpriteRenderer.sprite = Sprite.Create(
                texture: t,
                rect: new Rect(0, 0, t.width, t.height),
                pivot: new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 100,
                extrude: 1,
                meshType: SpriteMeshType.FullRect);

            c.Start();
            c.SerializedType = $"sprite.{imgPath},{material},{renderLayer}";
            ReferencedControllers.Add(c);
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
            ReferencedControllers.Add(c);
            return c;
        }

        public TextController CreateText(
            string font = "default",
            float fontSize = 40,
            float lineSpacing = 1,
            string alignment = "middlecenter",
            string renderLayer = "overlay")
        {
            GameObject obj = Instantiate(TextPrefab, ScreenCanvas.transform);
            obj.layer = GetLayer(renderLayer);
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

            c.SerializedType = $"text.{font},{fontSize},{lineSpacing},{alignment},{renderLayer}";
            ReferencedControllers.Add(c);
            return c;
        }

        public NoteGroupController GetNoteGroup(int tg)
        {
            try
            {
                var group = Services.Chart.GetTimingGroup(tg);
                NoteGroupController c = Instantiate(GroupPrefab, transform).GetComponent<NoteGroupController>();
                c.TimingGroup = group;
                c.SerializedType = $"tg.{group.GroupNumber}";
                ReferencedControllers.Add(c);
                return c;
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
                    return CreateImage(imgsplit[0], imgsplit[1], imgsplit[2]);
                case "canvas":
                    bool worldSpace = bool.Parse(def);
                    return CreateCanvas(worldSpace);
                case "sprite":
                    string[] spriteSplit = def.Split(',');
                    return CreateSprite(spriteSplit[0], spriteSplit[1], spriteSplit[2]);
                case "text":
                    string[] textSplit = def.Split(',');
                    return CreateText(
                        textSplit[0],
                        float.Parse(textSplit[1]),
                        float.Parse(textSplit[2]),
                        textSplit[3],
                        textSplit[4]);
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

        private int GetLayer(string name)
        {
            switch (name)
            {
                case "overlay":
                    return overlayLayer;
                case "notes":
                    return notesLayer;
                case "background":
                    return backgroundLayer;
                default:
                    return overlayLayer;
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