using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.Utility.Parser;
using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;

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
                Services.Scenecontrol.AddReferencedController(gameplayCamera);
                return gameplayCamera;
            }
        }
        [SerializeField] private TextController combo;
        public TextController Combo
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(combo);
                return combo;
            }
        }
        [SerializeField] private TextController score;
        public TextController Score
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(score);
                return score;
            }
        }
        [SerializeField] private ImageController jacket;
        public ImageController Jacket
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(jacket);
                return jacket;
            }
        }
        [SerializeField] private TitleController title;
        public TitleController Title
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(title);
                return title;
            }
        }
        [SerializeField] private ComposerController composer;
        public ComposerController Composer
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(composer);
                return composer;
            }
        }
        [SerializeField] private DifficultyController difficultyText;
        public DifficultyController DifficultyText
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(difficultyText);
                return difficultyText;
            }
        }
        [SerializeField] private ImageController difficultyBackground;
        public ImageController DifficultyBackground
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(difficultyBackground);
                return difficultyBackground;
            }
        }
        [SerializeField] private CanvasController hUD;
        public CanvasController HUD
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(hUD);
                return hUD;
            }
        }
        public CanvasController hud => HUD;
        [SerializeField] private InfoPanelController infoPanel;
        public InfoPanelController InfoPanel
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(infoPanel);
                return infoPanel;
            }
        }
        [SerializeField] private ImageController pauseButton;
        public ImageController PauseButton
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(pauseButton);
                return pauseButton;
            }
        }
        [SerializeField] private ImageController background;
        public ImageController Background
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(background);
                return background;
            }
        }
        [SerializeField] private SpriteController videoBackground;
        public SpriteController VideoBackground
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(videoBackground);
                return videoBackground;
            }
        }
        [SerializeField] private TrackController track;
        public TrackController Track
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(track);
                return track;
            }
        }
        [SerializeField] private SpriteController singleLineL;
        public SpriteController SingleLineL
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(singleLineL);
                return singleLineL;
            }
        }
        [SerializeField] private SpriteController singleLineR;
        public SpriteController SingleLineR
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(singleLineR);
                return singleLineR;
            }
        }
        [SerializeField] private GlowingSpriteController skyInputLine;
        public GlowingSpriteController SkyInputLine
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(skyInputLine);
                return skyInputLine;
            }
        }
        [SerializeField] private GlowingSpriteController skyInputLabel;
        public GlowingSpriteController SkyInputLabel
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(skyInputLabel);
                return skyInputLabel;
            }
        }
        [SerializeField] private BeatlinesController beatlines;
        public BeatlinesController Beatlines
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(beatlines);
                return beatlines;
            }
        }
        [SerializeField] private SpriteController darken;
        public SpriteController Darken
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(darken);
                return darken;
            }
        }
        [SerializeField] private CanvasController worldCanvas;
        public CanvasController WorldCanvas
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(worldCanvas);
                return worldCanvas;
            }
        }
        [SerializeField] private CanvasController screenCanvas;
        public CanvasController ScreenCanvas
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(screenCanvas);
                return screenCanvas;
            }
        }
        [SerializeField] private CanvasController cameraCanvas;
        public CanvasController CameraCanvas
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(cameraCanvas);
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
#pragma warning restore

        private readonly Dictionary<SpriteDefinition, Sprite> spriteCache = new Dictionary<SpriteDefinition, Sprite>();
        private readonly Dictionary<int, NoteGroupController> noteGroups = new Dictionary<int, NoteGroupController>();

        public void ClearCache()
        {
            foreach (var pair in spriteCache)
            {
                Destroy(pair.Value);
            }

            spriteCache.Clear();
            noteGroups.Clear();
        }

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

        public ImageController CreateImage(string imgPath, string material = "default", string renderLayer = "overlay", Vector2? pivot = null)
        {
            GameObject obj = Instantiate(ImagePrefab, ScreenCanvas.transform);
            obj.layer = GetLayer(renderLayer);
            ImageController c = obj.GetComponent<ImageController>();
            c.Image.material = GetMaterial(material);

            Sprite sprite = GetSprite(
                new SpriteDefinition
                {
                    Path = Path.Combine(Services.Scenecontrol.ScenecontrolFolder, imgPath),
                    Pivot = pivot ?? new Vector2(0.5f, 0.5f),
                }).AsTask().Result;
            c.Image.sprite = sprite;

            c.SerializedType = $"image.{imgPath},{material},{renderLayer}";
            c.Start();
            Services.Scenecontrol.AddReferencedController(c);
            return c;
        }

        public SpriteController CreateSprite(string imgPath, string material = "default", string renderLayer = "overlay", Vector2? pivot = null)
        {
            GameObject obj = Instantiate(SpritePrefab, ScreenCanvas.transform);
            obj.layer = GetLayer(renderLayer);
            SpriteController c = obj.GetComponent<SpriteController>();
            c.SpriteRenderer.material = GetMaterial(material);

            Sprite sprite = GetSprite(
                new SpriteDefinition
                {
                    Path = Path.Combine(Services.Scenecontrol.ScenecontrolFolder, imgPath),
                    Pivot = pivot ?? new Vector2(0.5f, 0.5f),
                }).AsTask().Result;
            c.SpriteRenderer.sprite = sprite;

            c.SerializedType = $"sprite.{imgPath},{material},{renderLayer}";
            c.Start();
            Services.Scenecontrol.AddReferencedController(c);
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

            c.SerializedType = $"canvas.{worldSpace}";
            c.Start();
            Services.Scenecontrol.AddReferencedController(c);
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
            c.Start();
            Services.Scenecontrol.AddReferencedController(c);
            return c;
        }

        public NoteGroupController GetNoteGroup(int tg)
        {
            if (noteGroups.TryGetValue(tg, out NoteGroupController cached))
            {
                return cached;
            }

            try
            {
                var group = Services.Chart.GetTimingGroup(tg);
                NoteGroupController c = Instantiate(GroupPrefab, transform).GetComponent<NoteGroupController>();
                c.TimingGroup = group;
                c.SerializedType = $"tg.{group.GroupNumber}";
                c.Start();
                Services.Scenecontrol.AddReferencedController(c);
                noteGroups.Add(group.GroupNumber, c);
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

        private async UniTask<Sprite> GetSprite(SpriteDefinition definition)
        {
            if (spriteCache.TryGetValue(definition, out Sprite sprite))
            {
                return sprite;
            }

            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(Uri.EscapeUriString(definition.Path)))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    throw new IOException(I18n.S(
                        "Gameplay.Exception.ScenecontrolCantLoadSprite",
                        new Dictionary<string, object>()
                            {
                                { "Path", definition.Path },
                                { "Error", req.error },
                            }));
                }

                var t = DownloadHandlerTexture.GetContent(req);
                Sprite output = Sprite.Create(
                        texture: t,
                        rect: new Rect(0, 0, t.width, t.height),
                        pivot: definition.Pivot,
                        pixelsPerUnit: 100,
                        extrude: 1,
                        meshType: SpriteMeshType.FullRect);

                spriteCache.Add(definition, output);
                return output;
            }
        }

        private void Awake()
        {
            gameplayCamera.SerializedType = "camera";
            combo.SerializedType = "combo";
            score.SerializedType = "score";
            jacket.SerializedType = "jacket";
            title.SerializedType = "title";
            composer.SerializedType = "composer";
            difficultyText.SerializedType = "diff";
            difficultyBackground.SerializedType = "diffBg";
            hUD.SerializedType = "hud";
            infoPanel.SerializedType = "info";
            pauseButton.SerializedType = "pause";
            background.SerializedType = "bg";
            videoBackground.SerializedType = "videobg";
            track.SerializedType = "track";
            singleLineL.SerializedType = "singlelinel";
            singleLineR.SerializedType = "singleliner";
            skyInputLine.SerializedType = "skyinputline";
            skyInputLabel.SerializedType = "skyinputlabel";
            beatlines.SerializedType = "beatlines";
            darken.SerializedType = "darken";
            worldCanvas.SerializedType = "worldcanvas";
            screenCanvas.SerializedType = "screencanvas";
            cameraCanvas.SerializedType = "cameracanvas";
        }

        private struct SpriteDefinition
        {
            public string Path;
            public Vector2 Pivot;
        }
    }
}