using System.Collections.Generic;
using System.Linq;
using ArcCreate.Gameplay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ArcCreate.Gameplay.Skin
{
    public class SkinService : MonoBehaviour, ISkinService, ISkinControl
    {
        [Header("Objects")]
        [SerializeField] private SpriteRenderer singleLineL;
        [SerializeField] private SpriteRenderer singleLineR;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private SpriteRenderer trackExtraL;
        [SerializeField] private SpriteRenderer trackExtraR;
        [SerializeField] private SpriteRenderer track;
        [SerializeField] private SpriteRenderer trackExtraEdgeL;
        [SerializeField] private SpriteRenderer trackExtraEdgeR;
        [SerializeField] private SpriteRenderer[] criticalLines = new SpriteRenderer[4];
        [SerializeField] private SpriteSO jacketShadowSO;
        [SerializeField] private Image background;
        [SerializeField] private GameObject videoBackgroundRenderer;
        [SerializeField] private VideoPlayer videoBackground;

        [Header("Skin Options")]
        [SerializeField] private List<AlignmentOption> alignmentOptions;
        [SerializeField] private List<NoteSkinOption> noteSkinOptions;
        [SerializeField] private List<ParticleSkinOption> particleSkinOptions;
        [SerializeField] private List<TrackSkinOption> trackSkinOptions;
        [SerializeField] private List<AccentOption> accentOptions;
        [SerializeField] private List<SingleLineOption> singleLineOptions;

        [Header("Colors")]
        [SerializeField] private Color defaultTraceColor;
        [SerializeField] private Color defaultShadowColor;
        [SerializeField] private List<Color> defaultArcColors;
        [SerializeField] private List<Color> defaultArcLowColors;
        [SerializeField] private Color unknownArcColor;
        [SerializeField] private Color unknownArcLowColor;

        [Header("Arcs")]
        [SerializeField] private Material baseArcMaterial;
        [SerializeField] private Material baseArcHighlightMaterial;
        [SerializeField] private Material traceMaterial;
        private readonly List<Material> arcMaterials = new List<Material>();
        private readonly List<Material> arcHighlightMaterials = new List<Material>();

        private AlignmentOption currentAlignment;
        private NoteSkinOption currentNoteSkin;
        private string currentNoteOption;
        private string currentParticleOption;
        private string currentTrackOption;
        private string currentAccentOption;
        private string currentSingleLineOption;

        private Color holdEffectColorMin;
        private Color holdEffectColorMax;

        private int highColorShaderId;
        private int lowColorShaderId;
        private int shadowColorShaderId;

        public Sprite BackgroundSprite
        {
            get => background.sprite;
            set => background.sprite = value;
        }

        public string VideoBackgroundUrl
        {
            get => videoBackground.url;
            set
            {
                videoBackground.url = value;
                videoBackgroundRenderer.SetActive(string.IsNullOrEmpty(value));
            }
        }

        public string AlignmentSkin
        {
            get => currentAlignment.Name;
            set
            {
                AlignmentOption alignmentOpt = alignmentOptions.FirstOrDefault(opt => opt.Name == value);
                alignmentOpt = alignmentOpt != null ? alignmentOpt : alignmentOptions[0];
                currentAlignment = alignmentOpt;

                jacketShadowSO.Value = alignmentOpt.JacketShadowSkin;

                // A bit jank but yeah
                NoteSkin = currentNoteOption;
                ParticleSkin = currentParticleOption;
                TrackSkin = currentTrackOption;
                SingleLineSkin = currentSingleLineOption;
                AccentSkin = currentAccentOption;
            }
        }

        public string NoteSkin
        {
            get => currentNoteOption;
            set
            {
                currentNoteOption = value;
                NoteSkinOption noteSkinOpt = noteSkinOptions.FirstOrDefault(opt => opt.Name == value);
                noteSkinOpt = noteSkinOpt != null ? noteSkinOpt : currentAlignment.DefaultNoteOption;

                currentNoteSkin = noteSkinOpt;
                ReloadNoteSkin(Settings.InputMode.Value);
            }
        }

        public string ParticleSkin
        {
            get => currentParticleOption;
            set
            {
                currentParticleOption = value;
                ParticleSkinOption particleOpt = particleSkinOptions.FirstOrDefault(opt => opt.Name == value);
                particleOpt = particleOpt != null ? particleOpt : currentAlignment.DefaultParticleOption;

                Services.Particle.SetTapParticleSkin(particleOpt.ParticleSkin);
                holdEffectColorMax = particleOpt.HoldEffectColorMax;
                holdEffectColorMin = particleOpt.HoldEffectColorMin;
            }
        }

        public string TrackSkin
        {
            get => currentTrackOption;
            set
            {
                currentTrackOption = value;
                TrackSkinOption trackOpt = trackSkinOptions.FirstOrDefault(opt => opt.Name == value);
                trackOpt = trackOpt != null ? trackOpt : currentAlignment.DefaultTrackOption;

                track.sprite = trackOpt.TrackSkin;
                trackExtraEdgeL.sprite = trackOpt.TrackSkin;
                trackExtraEdgeR.sprite = trackOpt.TrackSkin;
                trackExtraL.sprite = trackOpt.TrackExtraSkin;
                trackExtraR.sprite = trackOpt.TrackExtraSkin;
            }
        }

        public string SingleLineSkin
        {
            get => currentSingleLineOption;
            set
            {
                currentSingleLineOption = value;
                SingleLineOption lineOpt = singleLineOptions.FirstOrDefault(opt => opt.Name == value);
                lineOpt = lineOpt != null ? lineOpt : currentAlignment.DefaultSingleLineOption;

                singleLineL.sprite = lineOpt.SingleLineSkin;
                singleLineL.enabled = lineOpt.Enable;
                singleLineR.sprite = lineOpt.SingleLineSkin;
                singleLineR.enabled = lineOpt.Enable;
            }
        }

        public string AccentSkin
        {
            get => currentAccentOption;
            set
            {
                currentAccentOption = value;
                AccentOption accentOpt = accentOptions.FirstOrDefault(opt => opt.Name == value);
                accentOpt = accentOpt != null ? accentOpt : currentAlignment.DefaultAccentOption;

                foreach (var c in criticalLines)
                {
                    c.sprite = accentOpt.CriticalLineSkin;
                }

                comboText.color = accentOpt.ComboColor;
                comboText.outlineColor = accentOpt.ComboColor;
            }
        }

        public Color DefaultTraceColor => defaultTraceColor;

        public Color DefaultShadowColor => defaultShadowColor;

        public List<Color> DefaultArcColors => defaultArcColors;

        public List<Color> DefaultArcLowColors => defaultArcLowColors;

        public Color UnknownArcColor => unknownArcColor;

        public Color UnknownArcLowColor => unknownArcLowColor;

        public Sprite GetTapSkin(Tap note)
            => currentNoteSkin.GetTapSkin(note);

        public (Sprite normal, Sprite highlight) GetHoldSkin(Hold note)
            => currentNoteSkin.GetHoldSkin(note);

        public (Mesh mesh, Material material) GetArcTapSkin(ArcTap note)
            => currentNoteSkin.GetArcTapSkin(note);

        public (Material normal, Material highlight, Sprite arcCap, Color psMin, Color psMax) GetArcSkin(Arc note)
        {
            Sprite arcCap = currentNoteSkin.GetArcCapSprite(note);

            if (note.IsVoid)
            {
                return (traceMaterial, traceMaterial, arcCap, holdEffectColorMax, holdEffectColorMin);
            }

            if (note.Color < 0 || note.Color >= arcMaterials.Count)
            {
                return (arcMaterials[0], arcHighlightMaterials[0], arcCap, holdEffectColorMax, holdEffectColorMin);
            }

            return (arcMaterials[note.Color], arcHighlightMaterials[note.Color], arcCap, holdEffectColorMax, holdEffectColorMin);
        }

        public (Sprite lane, Sprite extraLane) GetTrackSprite(string name)
        {
            TrackSkinOption trackSkinOpt = trackSkinOptions.FirstOrDefault(opt => opt.Name == name);
            trackSkinOpt = trackSkinOpt != null ? trackSkinOpt : trackSkinOptions.First();
            return (trackSkinOpt.TrackSkin, trackSkinOpt.TrackExtraSkin);
        }

        public void SetTraceColor(Color color, Color shadow)
        {
            traceMaterial.SetColor(highColorShaderId, color);
            traceMaterial.SetColor(lowColorShaderId, color);
            traceMaterial.SetColor(shadowColorShaderId, shadow);
        }

        public void SetArcColors(List<Color> arcs, List<Color> arcLows, Color shadow)
        {
            arcMaterials.ForEach(Destroy);
            arcHighlightMaterials.ForEach(Destroy);
            arcMaterials.Clear();
            arcHighlightMaterials.Clear();

            int max = Mathf.Min(arcs.Count, arcLows.Count);
            for (int i = 0; i < max; i++)
            {
                Material mat = Instantiate(baseArcMaterial);
                mat.SetColor(highColorShaderId, arcs[i]);
                mat.SetColor(lowColorShaderId, arcLows[i]);
                mat.SetColor(shadowColorShaderId, shadow);
                arcMaterials.Add(mat);

                Material hmat = Instantiate(baseArcHighlightMaterial);
                hmat.SetColor(highColorShaderId, arcs[i]);
                hmat.SetColor(lowColorShaderId, arcLows[i]);
                hmat.SetColor(shadowColorShaderId, shadow);
                arcHighlightMaterials.Add(hmat);
            }
        }

        public void ResetTraceColors()
        {
            SetTraceColor(defaultTraceColor, defaultShadowColor);
        }

        public void ResetArcColors()
        {
            SetArcColors(defaultArcColors, defaultArcLowColors, defaultShadowColor);
        }

        private void Awake()
        {
            highColorShaderId = Shader.PropertyToID("_Color");
            lowColorShaderId = Shader.PropertyToID("_LowColor");
            shadowColorShaderId = Shader.PropertyToID("_ShadowColor");
            Settings.InputMode.OnValueChanged.AddListener(ReloadNoteSkin);

            currentAlignment = alignmentOptions[0];
            currentNoteSkin = currentAlignment.DefaultNoteOption;

            ResetTraceColors();
            ResetArcColors();
        }

        private void OnDestroy()
        {
            Settings.InputMode.OnValueChanged.RemoveListener(ReloadNoteSkin);
        }

        private void ReloadNoteSkin(int inputMode)
        {
            Services.Chart.ReloadSkin();
        }
    }
}