using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.ChartFormat;
using ArcCreate.Utility.Lua;
using ArcCreate.Utility.Parser;
using Cysharp.Threading.Tasks;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyDoc("Class for interacting with the scene")]
    public class Scene : MonoBehaviour
    {
#pragma warning disable
        [Header("Internal")]
        [SerializeField] private CameraController gameplayCamera;
        [EmmyDoc("Gets the gameplay camera controller")]
        public CameraController GameplayCamera
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(gameplayCamera);
                return gameplayCamera;
            }
        }
        [SerializeField] private TextController combo;
        [EmmyDoc("Gets the combo text controller")]
        public TextController Combo
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(combo);
                return combo;
            }
        }
        [SerializeField] private TextController score;
        [EmmyDoc("Gets the score text controller")]
        public TextController Score
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(score);
                return score;
            }
        }
        [SerializeField] private ImageController jacket;
        [EmmyDoc("Gets the jacket art image controller")]
        public ImageController Jacket
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(jacket);
                return jacket;
            }
        }
        [SerializeField] private TitleController title;
        [EmmyDoc("Gets the title text controller")]
        public TitleController Title
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(title);
                return title;
            }
        }
        [SerializeField] private ComposerController composer;
        [EmmyDoc("Gets the composer text controller")]
        public ComposerController Composer
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(composer);
                return composer;
            }
        }
        [SerializeField] private DifficultyController difficultyText;
        [EmmyDoc("Gets the difficulty text controller")]
        public DifficultyController DifficultyText
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(difficultyText);
                return difficultyText;
            }
        }
        [SerializeField] private ImageController difficultyBackground;
        [EmmyDoc("Gets the difficulty background image controller")]
        public ImageController DifficultyBackground
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(difficultyBackground);
                return difficultyBackground;
            }
        }
        [SerializeField] private CanvasController hUD;
        [EmmyDoc("Gets the HUD canvas controller")]
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
        [EmmyDoc("Gets the info panel image controller")]
        public InfoPanelController InfoPanel
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(infoPanel);
                return infoPanel;
            }
        }
        [SerializeField] private ImageController pauseButton;
        [EmmyDoc("Gets the pause button image controller")]
        public ImageController PauseButton
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(pauseButton);
                return pauseButton;
            }
        }
        [SerializeField] private ImageController background;
        [EmmyDoc("Gets the background image controller")]
        public ImageController Background
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(background);
                return background;
            }
        }
        [SerializeField] private SpriteController videoBackground;
        [EmmyDoc("Gets the video background sprite controller")]
        public SpriteController VideoBackground
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(videoBackground);
                return videoBackground;
            }
        }
        [SerializeField] private TrackController track;
        [EmmyDoc("Gets the track sprite controller")]
        public TrackController Track
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(track);
                return track;
            }
        }
        [SerializeField] private SpriteController singleLineL;
        [EmmyDoc("Gets the left single line sprite controller")]
        public SpriteController SingleLineL
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(singleLineL);
                return singleLineL;
            }
        }
        [SerializeField] private SpriteController singleLineR;
        [EmmyDoc("Gets the right single line sprite controller")]
        public SpriteController SingleLineR
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(singleLineR);
                return singleLineR;
            }
        }
        [SerializeField] private GlowingSpriteController skyInputLine;
        [EmmyDoc("Gets the sky input line sprite controller")]
        public GlowingSpriteController SkyInputLine
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(skyInputLine);
                return skyInputLine;
            }
        }
        [SerializeField] private GlowingSpriteController skyInputLabel;
        [EmmyDoc("Gets the sky input label sprite controller")]
        public GlowingSpriteController SkyInputLabel
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(skyInputLabel);
                return skyInputLabel;
            }
        }
        [SerializeField] private BeatlinesController beatlines;
        [EmmyDoc("Gets the beatlines display controller")]
        public BeatlinesController Beatlines
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(beatlines);
                return beatlines;
            }
        }
        [SerializeField] private SpriteController darken;
        [EmmyDoc("Gets the background darkening sprite controller")]
        public SpriteController Darken
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(darken);
                return darken;
            }
        }
        [SerializeField] private CanvasController worldCanvas;
        [EmmyDoc("Gets the world canvas controller")]
        public CanvasController WorldCanvas
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(worldCanvas);
                return worldCanvas;
            }
        }
        [SerializeField] private CanvasController screenCanvas;
        [EmmyDoc("Gets the screen canvas controller")]
        public CanvasController ScreenCanvas
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(screenCanvas);
                return screenCanvas;
            }
        }
        [SerializeField] private CanvasController cameraCanvas;
        [EmmyDoc("Gets the camera canvas controller")]
        public CanvasController CameraCanvas
        {
            get
            {
                Services.Scenecontrol.AddReferencedController(cameraCanvas);
                return cameraCanvas;
            }
        }

        [MoonSharpHidden] public Transform CanvasParent;

        [Header("Prefab")]
        [MoonSharpHidden] public GameObject ImagePrefab;
        [MoonSharpHidden] public GameObject CanvasPrefab;
        [MoonSharpHidden] public GameObject SpritePrefab;
        [MoonSharpHidden] public GameObject TextPrefab;
        [MoonSharpHidden] public GameObject GroupPrefab;

        [Header("Materials")]
        [MoonSharpHidden] public Material DefaultMaterial;
        [MoonSharpHidden] public Material ColorBurnMaterial;
        [MoonSharpHidden] public Material ColorDodgeMaterial;
        [MoonSharpHidden] public Material DarkenMaterial;
        [MoonSharpHidden] public Material DifferenceMaterial;
        [MoonSharpHidden] public Material ExclusionMaterial;
        [MoonSharpHidden] public Material FastAddMaterial;
        [MoonSharpHidden] public Material FastDarkenMaterial;
        [MoonSharpHidden] public Material FastLightenMaterial;
        [MoonSharpHidden] public Material FastMultiplyMaterial;
        [MoonSharpHidden] public Material FastScreenMaterial;
        [MoonSharpHidden] public Material HardLightMaterial;
        [MoonSharpHidden] public Material LightenMaterial;
        [MoonSharpHidden] public Material LinearBurnMaterial;
        [MoonSharpHidden] public Material LinearDodgeMaterial;
        [MoonSharpHidden] public Material LinearLightMaterial;
        [MoonSharpHidden] public Material MultiplyMaterial;
        [MoonSharpHidden] public Material OverlayMaterial;
        [MoonSharpHidden] public Material ScreenMaterial;
        [MoonSharpHidden] public Material SoftLightMaterial;
        [MoonSharpHidden] public Material SubtractMaterial;
        [MoonSharpHidden] public Material VividLightMaterial;

        [SerializeField] private int overlayLayer;
        [SerializeField] private int notesLayer;
        [SerializeField] private int backgroundLayer;

        [MoonSharpHidden] public List<Controller> DisabledByDefault;
        private IFileAccessWrapper customFileAccess;
#pragma warning restore

        private readonly Dictionary<SpriteDefinition, Sprite> spriteCache = new Dictionary<SpriteDefinition, Sprite>();
        private readonly Dictionary<int, NoteGroupController> noteGroups = new Dictionary<int, NoteGroupController>();
        private readonly List<UniTask<Sprite>> spriteTasks = new List<UniTask<Sprite>>();

        [MoonSharpHidden]
        public void SetFileAccess(IFileAccessWrapper fileAccess)
        {
            customFileAccess = fileAccess;
        }

        [MoonSharpHidden]
        public void ClearCache()
        {
            foreach (var pair in spriteCache)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value);

                    if (pair.Value.texture != null)
                    {
                        Destroy(pair.Value.texture);
                    }
                }
            }

            spriteCache.Clear();
            noteGroups.Clear();
        }

        [MoonSharpHidden]
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

#pragma warning disable
        [EmmyDoc("Creates an image controller from an image file")]
        public ImageController CreateImage(
            string imgPath,
            [EmmyChoice("default", "colorburn", "colordodge", "darken",
                        "difference", "exclusion", "add", "fastadd", "fastdarken",
                        "fastlighten", "fastmultiply", "fastscreen", "hardlight",
                        "lighten", "linearburn", "lineardodge", "linearlight",
                        "multiply", "overlay", "screen", "softlight",
                        "subtract", "vividlight")]
            string material = "default",
            [EmmyChoice("overlay", "notes", "background")]
            string renderLayer = "overlay",
            XY? pivot = null)
#pragma warning restore
        {
            GameObject obj = Instantiate(ImagePrefab, ScreenCanvas.transform);
            obj.layer = GetLayer(renderLayer);
            ImageController c = obj.GetComponent<ImageController>();
            c.Image.material = GetMaterial(material);
            Vector2 pivotVec = pivot?.ToVector() ?? new Vector2(0.5f, 0.5f);

            spriteTasks.Add(GetSprite(
                new SpriteDefinition
                {
                    Uri = customFileAccess?.GetFileUri(Path.Combine("Scenecontrol", imgPath)) ?? Path.Combine(Services.Scenecontrol.ScenecontrolFolder, imgPath),
                    Pivot = pivotVec,
                }).ContinueWith(sprite => c.Image.sprite = sprite));

            c.SerializedType = $"image.{imgPath},{material},{renderLayer},{pivotVec.x},{pivotVec.y}";
            c.Start();
            Services.Scenecontrol.AddReferencedController(c);
            return c;
        }

#pragma warning disable
        [EmmyDoc("Creates a sprite controller from an image file")]
        public SpriteController CreateSprite(
            string imgPath,
            [EmmyChoice("default", "colorburn", "colordodge", "darken",
                        "difference", "exclusion", "add", "fastadd", "fastdarken",
                        "fastlighten", "fastmultiply", "fastscreen", "hardlight",
                        "lighten", "linearburn", "lineardodge", "linearlight",
                        "multiply", "overlay", "screen", "softlight",
                        "subtract", "vividlight")]
            string material = "default",
            [EmmyChoice("overlay", "notes", "background")]
            string renderLayer = "overlay",
            XY? pivot = null)
#pragma warning restore
        {
            GameObject obj = Instantiate(SpritePrefab, ScreenCanvas.transform);
            obj.layer = GetLayer(renderLayer);
            SpriteController c = obj.GetComponent<SpriteController>();
            c.SpriteRenderer.material = GetMaterial(material);
            Vector2 pivotVec = pivot?.ToVector() ?? new Vector2(0.5f, 0.5f);

            spriteTasks.Add(GetSprite(
                new SpriteDefinition
                {
                    Uri = customFileAccess?.GetFileUri(Path.Combine("Scenecontrol", imgPath)) ?? Path.Combine(Services.Scenecontrol.ScenecontrolFolder, imgPath),
                    Pivot = pivotVec,
                }).ContinueWith(sprite => c.SpriteRenderer.sprite = sprite));

            c.SerializedType = $"sprite.{imgPath},{material},{renderLayer},{pivotVec.x},{pivotVec.y}";
            c.Start();
            Services.Scenecontrol.AddReferencedController(c);
            return c;
        }

        [EmmyDoc("Creates a canvas either in world space or in screen space")]
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

            c.SerializedType = $"canvas.{(worldSpace ? "true" : "false")}";
            c.Start();
            Services.Scenecontrol.AddReferencedController(c);
            return c;
        }

        [EmmyDoc("Creates a new text controller")]
        public TextController CreateText(
            string font = "default",
            float fontSize = 40,
            float lineSpacing = 1,
            [EmmyChoice("upperleft", "uppercenter", "upperright", "middleleft", "middlecenter", "middlerigh", "lowerleft", "lowercenter", "lowerright")]
            string alignment = "middlecenter",
            [EmmyChoice("overlay", "notes", "background")]
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

        [EmmyDoc("Creates a note group controller for a timing group")]
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

        [MoonSharpHidden]
        public Controller CreateFromTypeName(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return null;
            }

            bool copy = type[0] == '$';
            if (copy)
            {
                type = type.Substring(1);
            }

            string def = string.Empty;
            string arg = string.Empty;
            StringParser parser = new StringParser(type);

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

        [MoonSharpHidden]
        public async UniTask WaitForTasksComplete()
        {
            while (true)
            {
                bool complete = true;
                foreach (var task in spriteTasks)
                {
                    if (task.Status == UniTaskStatus.Pending)
                    {
                        complete = false;
                        break;
                    }

                    if (task.Status == UniTaskStatus.Faulted)
                    {
                        throw new Exception("Could not load a sprite for scenecontrol (Unknown error)");
                    }
                }

                if (complete)
                {
                    spriteTasks.Clear();
                    return;
                }
                else
                {
                    await UniTask.NextFrame();
                }
            }
        }

        [MoonSharpHidden]
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
                case "divline01":
                    return track.DivideLine01;
                case "divline12":
                    return track.DivideLine12;
                case "divline23":
                    return track.DivideLine23;
                case "divline34":
                    return track.DivideLine34;
                case "divline45":
                    return track.DivideLine45;
                case "critline0":
                    return track.CriticalLine0;
                case "critline1":
                    return track.CriticalLine1;
                case "critline2":
                    return track.CriticalLine2;
                case "critline3":
                    return track.CriticalLine3;
                case "critline4":
                    return track.CriticalLine4;
                case "critline5":
                    return track.CriticalLine5;
                case "edgeextraL":
                    return track.EdgeExtraL;
                case "edgeextraR":
                    return track.EdgeExtraR;
                case "extraL":
                    return track.ExtraL;
                case "extraR":
                    return track.ExtraR;
                case "image":
                    string[] imgsplit = arg.Split(',');
                    return CreateImage(imgsplit[0], imgsplit[1], imgsplit[2], new XY(float.Parse(imgsplit[3]), float.Parse(imgsplit[4])));
                case "canvas":
                    bool worldSpace = def.ToLower() == "true";
                    return CreateCanvas(worldSpace);
                case "sprite":
                    string[] spriteSplit = arg.Split(',');
                    return CreateSprite(spriteSplit[0], spriteSplit[1], spriteSplit[2], new XY(float.Parse(spriteSplit[3]), float.Parse(spriteSplit[4])));
                case "text":
                    string[] textSplit = arg.Split(',');
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
            if (spriteCache.ContainsKey(definition))
            {
                while (spriteCache[definition] == null)
                {
                    await UniTask.NextFrame();
                }

                return spriteCache[definition];
            }

            spriteCache.Add(definition, null);
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(Uri.EscapeUriString(definition.Uri.Replace("\\", "/"))))
            {
                try
                {
                    await req.SendWebRequest();

                    var t = DownloadHandlerTexture.GetContent(req);
                    Sprite output = Sprite.Create(
                            texture: t,
                            rect: new Rect(0, 0, t.width, t.height),
                            pivot: definition.Pivot,
                            pixelsPerUnit: 100,
                            extrude: 1,
                            meshType: SpriteMeshType.FullRect);

                    spriteCache[definition] = output;
                    return output;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw new IOException(I18n.S(
                        "Gameplay.Exception.ScenecontrolCantLoadSprite",
                        new Dictionary<string, object>()
                            {
                                { "Path", definition.Uri },
                                { "Error", e.Message + e.StackTrace },
                            }));
                }
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
            public string Uri;
            public Vector2 Pivot;
        }
    }
}