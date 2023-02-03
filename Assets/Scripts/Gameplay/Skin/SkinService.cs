using System.Collections.Generic;
using System.Linq;
using ArcCreate.Gameplay.Data;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Skin
{
    public class SkinService : MonoBehaviour, ISkinService, ISkinControl
    {
        [SerializeField] private GameplayData gameplayData;

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
        private readonly List<Color> arcHeightIndicatorColors = new List<Color>();

        private AlignmentOption currentAlignment;
        private NoteSkinOption currentNoteSkin;
        private string currentNoteOption;
        private string currentParticleOption;
        private string currentTrackOption;
        private string currentAccentOption;
        private string currentSingleLineOption;

        private int highColorShaderId;
        private int lowColorShaderId;
        private int shadowColorShaderId;
        private int redValueShaderId;

        private Color currentComboColor;

        public string AlignmentSkin
        {
            get => currentAlignment.Name;
            set
            {
                AlignmentOption alignmentOpt = alignmentOptions.FirstOrDefault(opt => opt.Name == value);
                alignmentOpt = alignmentOpt != null ? alignmentOpt : alignmentOptions[0];
                currentAlignment = alignmentOpt;

                jacketShadowSO.Value = alignmentOpt.JacketShadowSkin.Value;

                // A bit jank but yeah
                NoteSkin = currentNoteOption;
                ParticleSkin = currentParticleOption;
                TrackSkin = currentTrackOption;
                SingleLineSkin = currentSingleLineOption;
                AccentSkin = currentAccentOption;
                gameplayData.NotifySkinValuesChange();
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
                gameplayData.NotifySkinValuesChange();
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

                Services.Particle.SetTapParticleSkin(particleOpt.ParticleSkin.Value);
                Services.Particle.SetLongParticleSkin(particleOpt.HoldEffectColorMin, particleOpt.HoldEffectColorMax);
                gameplayData.NotifySkinValuesChange();
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

                track.sprite = trackOpt.TrackSkin.Value;
                trackExtraEdgeL.sprite = trackOpt.TrackSkin.Value;
                trackExtraEdgeR.sprite = trackOpt.TrackSkin.Value;
                trackExtraL.sprite = trackOpt.TrackExtraSkin.Value;
                trackExtraR.sprite = trackOpt.TrackExtraSkin.Value;
                gameplayData.NotifySkinValuesChange();
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

                singleLineL.sprite = lineOpt.SingleLineSkin.Value;
                singleLineL.enabled = lineOpt.Enable;
                singleLineR.sprite = lineOpt.SingleLineSkin.Value;
                singleLineR.enabled = lineOpt.Enable;
                gameplayData.NotifySkinValuesChange();
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
                    c.sprite = accentOpt.CriticalLineSkin.Value;
                }

                comboText.color = accentOpt.ComboColor;
                comboText.outlineColor = accentOpt.ComboColor;
                currentComboColor = accentOpt.ComboColor;
                gameplayData.NotifySkinValuesChange();
            }
        }

        public Color ComboColor => currentComboColor;

        public Color DefaultTraceColor => defaultTraceColor;

        public Color DefaultShadowColor => defaultShadowColor;

        public List<Color> DefaultArcColors => defaultArcColors;

        public List<Color> DefaultArcLowColors => defaultArcLowColors;

        public Color UnknownArcColor => unknownArcColor;

        public Color UnknownArcLowColor => unknownArcLowColor;

        public Sprite DefaultBackground => currentAlignment.DefaultBackground.Value;

        public Sprite GetTapSkin(Tap note)
            => currentNoteSkin.GetTapSkin(note);

        public (Sprite normal, Sprite highlight) GetHoldSkin(Hold note)
            => currentNoteSkin.GetHoldSkin(note);

        public (Mesh mesh, Material material) GetArcTapSkin(ArcTap note)
            => currentNoteSkin.GetArcTapSkin(note);

        public (Material normal, Material highlight, Sprite arcCap, Color heightIndicatorColor) GetArcSkin(Arc note)
        {
            Sprite arcCap = currentNoteSkin.GetArcCapSprite(note);

            if (note.IsTrace)
            {
                return (traceMaterial, traceMaterial, arcCap, Color.white);
            }

            if (note.Color < 0 || note.Color >= arcMaterials.Count)
            {
                return (arcMaterials[0], arcHighlightMaterials[0], arcCap, arcHeightIndicatorColors[0]);
            }

            return (arcMaterials[note.Color], arcHighlightMaterials[note.Color], arcCap, arcHeightIndicatorColors[note.Color]);
        }

        public (Sprite lane, Sprite extraLane) GetTrackSprite(string name)
        {
            TrackSkinOption trackSkinOpt = trackSkinOptions.FirstOrDefault(opt => opt.Name == name);
            trackSkinOpt = trackSkinOpt != null ? trackSkinOpt : trackSkinOptions.First();
            return (trackSkinOpt.TrackSkin.Value, trackSkinOpt.TrackExtraSkin.Value);
        }

        public void SetTraceColor(Color color, Color shadow)
        {
            traceMaterial.SetColor(highColorShaderId, color);
            traceMaterial.SetColor(shadowColorShaderId, shadow);
        }

        public void SetArcColors(List<Color> arcs, List<Color> arcLows, Color shadow)
        {
            arcMaterials.ForEach(Destroy);
            arcHighlightMaterials.ForEach(Destroy);
            arcMaterials.Clear();
            arcHighlightMaterials.Clear();
            arcHeightIndicatorColors.Clear();

            int max = Mathf.Min(arcs.Count, arcLows.Count);
            for (int i = 0; i < max; i++)
            {
                Material mat = Instantiate(baseArcMaterial);
                mat.SetColor(highColorShaderId, arcs[i]);
                mat.SetColor(lowColorShaderId, arcLows[i]);
                mat.SetColor(shadowColorShaderId, shadow);
                mat.renderQueue = mat.renderQueue + max - i;
                arcMaterials.Add(mat);

                Material hmat = Instantiate(baseArcHighlightMaterial);
                hmat.SetColor(highColorShaderId, arcs[i]);
                hmat.SetColor(lowColorShaderId, arcLows[i]);
                hmat.SetColor(shadowColorShaderId, shadow);
                hmat.renderQueue = hmat.renderQueue + max - i;
                arcHighlightMaterials.Add(hmat);

                arcHeightIndicatorColors.Add(arcs[i]);
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

        public void ApplyRedArcValue(int color, float value)
        {
            if (color < 0 || color >= arcMaterials.Count)
            {
                return;
            }

            Material mat = arcMaterials[color];
            mat.SetFloat(redValueShaderId, value);
        }

        private void Awake()
        {
            highColorShaderId = Shader.PropertyToID("_Color");
            lowColorShaderId = Shader.PropertyToID("_LowColor");
            shadowColorShaderId = Shader.PropertyToID("_ShadowColor");
            redValueShaderId = Shader.PropertyToID("_RedValue");
            Settings.InputMode.OnValueChanged.AddListener(ReloadNoteSkin);

            currentAlignment = alignmentOptions[0];
            currentNoteSkin = currentAlignment.DefaultNoteOption;
            currentComboColor = currentAlignment.DefaultAccentOption.ComboColor;

            RegisterExternalSkin();
            LoadExternalSkin().Forget();
            ResetTraceColors();
            ResetArcColors();
        }

        private void RegisterExternalSkin()
        {
            foreach (var opt in alignmentOptions)
            {
                opt.RegisterExternalSkin();
            }

            foreach (var opt in noteSkinOptions)
            {
                opt.RegisterExternalSkin();
            }

            foreach (var opt in particleSkinOptions)
            {
                opt.RegisterExternalSkin();
            }

            foreach (var opt in trackSkinOptions)
            {
                opt.RegisterExternalSkin();
            }

            foreach (var opt in accentOptions)
            {
                opt.RegisterExternalSkin();
            }

            foreach (var opt in singleLineOptions)
            {
                opt.RegisterExternalSkin();
            }
        }

        private async UniTask LoadExternalSkin()
        {
            foreach (var opt in alignmentOptions)
            {
                await opt.LoadExternalSkin();
            }

            foreach (var opt in noteSkinOptions)
            {
                await opt.LoadExternalSkin();
            }

            foreach (var opt in particleSkinOptions)
            {
                await opt.LoadExternalSkin();
            }

            foreach (var opt in trackSkinOptions)
            {
                await opt.LoadExternalSkin();
            }

            foreach (var opt in accentOptions)
            {
                await opt.LoadExternalSkin();
            }

            foreach (var opt in singleLineOptions)
            {
                await opt.LoadExternalSkin();
            }

            ReapplySkin();
        }

        private void ReapplySkin()
        {
            AlignmentSkin = AlignmentSkin;
            NoteSkin = NoteSkin;
            ParticleSkin = ParticleSkin;
            TrackSkin = TrackSkin;
            SingleLineSkin = SingleLineSkin;
            AccentSkin = AccentSkin;
        }

        private void OnDestroy()
        {
            Settings.InputMode.OnValueChanged.RemoveListener(ReloadNoteSkin);

            foreach (var opt in alignmentOptions)
            {
                opt.UnloadExternalSkin();
            }

            foreach (var opt in noteSkinOptions)
            {
                opt.UnloadExternalSkin();
            }

            foreach (var opt in particleSkinOptions)
            {
                opt.UnloadExternalSkin();
            }

            foreach (var opt in trackSkinOptions)
            {
                opt.UnloadExternalSkin();
            }

            foreach (var opt in accentOptions)
            {
                opt.UnloadExternalSkin();
            }

            foreach (var opt in singleLineOptions)
            {
                opt.UnloadExternalSkin();
            }
        }

        private void ReloadNoteSkin(int inputMode)
        {
            Services.Chart?.ReloadSkin();
        }
    }
}