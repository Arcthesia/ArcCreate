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
        [SerializeField] private Material shadowMaterial;
        [SerializeField] private Texture defaultHeightIndicator;
        [SerializeField] private Texture defaultArctapShadow;
        private readonly List<Material> arcMaterials = new List<Material>();
        private readonly List<Material> arcHighlightMaterials = new List<Material>();
        private readonly List<Color> arcHeightIndicatorColors = new List<Color>();
        private readonly List<float> redArcValues = new List<float>();
        private float unknownRedArcValue = 0;
        private ExternalTexture arcTexture;
        private ExternalTexture arcTextureHighlight;
        private ExternalTexture arctapShadowTexture;
        private ExternalTexture heightIndicatorTexture;

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
                Services.Particle.SetLongParticleSkin(
                    particleOpt.HoldEffectColorMin,
                    particleOpt.HoldEffectColorMax,
                    particleOpt.HoldEffectFromGradient,
                    particleOpt.HoldEffectToGradient);
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
                Color outline = accentOpt.ComboColor;
                outline.a = 0.5f;
                comboText.gameObject.SetActive(false);
                comboText.fontSharedMaterial.SetColor("_OutlineColor", outline);
                comboText.gameObject.SetActive(true);
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

        public (Texture texture, Color connectionLineColor) GetTapSkin(Tap note)
        {
            switch (note.TimingGroupInstance.GroupProperties.SkinOverride)
            {
                case NoteSkinOverride.Default:
                    return currentNoteSkin.GetTapSkin(note);
                case NoteSkinOverride.Light:
                    return noteSkinOptions[0].GetTapSkin(note);
                case NoteSkinOverride.Conflict:
                    return noteSkinOptions[1].GetTapSkin(note);
                default:
                    return currentNoteSkin.GetTapSkin(note);
            }
        }

        public (Texture normal, Texture highlight) GetHoldSkin(Hold note)
        {
            switch (note.TimingGroupInstance.GroupProperties.SkinOverride)
            {
                case NoteSkinOverride.Default:
                    return currentNoteSkin.GetHoldSkin(note);
                case NoteSkinOverride.Light:
                    return noteSkinOptions[0].GetHoldSkin(note);
                case NoteSkinOverride.Conflict:
                    return noteSkinOptions[1].GetHoldSkin(note);
                default:
                    return currentNoteSkin.GetHoldSkin(note);
            }
        }

        public Texture GetArcTapSkin(ArcTap note)
        {
            return currentNoteSkin.GetArcTapSkin(note);
        }

        public (Texture arcCap, Color heightIndicatorColor) GetArcSkin(Arc note)
        {
            Texture arcCap = currentNoteSkin.GetArcCapSprite(note);

            if (note.IsTrace)
            {
                return (arcCap, Color.clear);
            }

            if (note.Color < 0 || note.Color >= arcHeightIndicatorColors.Count)
            {
                return (arcCap, unknownArcColor);
            }

            return (arcCap, arcHeightIndicatorColors[note.Color]);
        }

        public float GetRedArcValue(int color)
        {
            if (color < 0 || color >= redArcValues.Count)
            {
                return unknownRedArcValue;
            }

            return redArcValues[color];
        }

        public (Sprite lane, Sprite extraLane) GetTrackSprite(string name)
        {
            TrackSkinOption trackSkinOpt = trackSkinOptions.FirstOrDefault(opt => opt.Name == name);
            trackSkinOpt = trackSkinOpt != null ? trackSkinOpt : trackSkinOptions.First();
            return (trackSkinOpt.TrackSkin.Value, trackSkinOpt.TrackExtraSkin.Value);
        }

        public void SetTraceColor(Color color)
        {
            traceMaterial.SetColor(highColorShaderId, color);
            Services.Render.SetTraceMaterial(traceMaterial);
        }

        public void SetArcColors(List<Color> arcs, List<Color> arcLows)
        {
            arcMaterials.ForEach(Destroy);
            arcHighlightMaterials.ForEach(Destroy);
            arcMaterials.Clear();
            arcHighlightMaterials.Clear();
            arcHeightIndicatorColors.Clear();
            redArcValues.Clear();

            int max = Mathf.Min(arcs.Count, arcLows.Count);
            for (int i = 0; i < max; i++)
            {
                Material mat = Instantiate(baseArcMaterial);
                mat.SetColor(highColorShaderId, arcs[i]);
                mat.SetColor(lowColorShaderId, arcLows[i]);
                arcMaterials.Add(mat);

                Material hmat = Instantiate(baseArcHighlightMaterial);
                hmat.SetColor(highColorShaderId, arcs[i]);
                hmat.SetColor(lowColorShaderId, arcLows[i]);
                arcHighlightMaterials.Add(hmat);

                arcHeightIndicatorColors.Add(arcs[i]);
                redArcValues.Add(0);
            }

            Services.Render.SetArcMaterials(arcMaterials, arcHighlightMaterials);
        }

        public void SetShadowColor(Color color)
        {
            shadowMaterial.SetColor(shadowColorShaderId, color);
            Services.Render.SetShadowMaterial(shadowMaterial);
        }

        public void ResetTraceColors()
        {
            SetTraceColor(defaultTraceColor);
        }

        public void ResetArcColors()
        {
            SetArcColors(defaultArcColors, defaultArcLowColors);
        }

        public void ResetShadowColor()
        {
            SetShadowColor(defaultShadowColor);
        }

        public void ApplyRedArcValue(int color, float value)
        {
            if (color < 0 || color >= redArcValues.Count)
            {
                unknownRedArcValue = value;
            }

            redArcValues[color] = value;
        }

        private void Awake()
        {
            highColorShaderId = Shader.PropertyToID("_Color");
            lowColorShaderId = Shader.PropertyToID("_LowColor");
            shadowColorShaderId = Shader.PropertyToID("_ShadowColor");
            Settings.InputMode.OnValueChanged.AddListener(ReloadNoteSkin);

            baseArcMaterial = Instantiate(baseArcMaterial);
            baseArcHighlightMaterial = Instantiate(baseArcHighlightMaterial);
            traceMaterial = Instantiate(traceMaterial);
            shadowMaterial = Instantiate(shadowMaterial);

            currentAlignment = alignmentOptions[0];
            currentNoteSkin = currentAlignment.DefaultNoteOption;
            currentComboColor = currentAlignment.DefaultAccentOption.ComboColor;

            RegisterExternalSkin();
            LoadExternalSkin().Forget();
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

            arcTexture = new ExternalTexture(baseArcMaterial.mainTexture, "Note");
            arcTextureHighlight = new ExternalTexture(baseArcHighlightMaterial.mainTexture, "Note");
            arctapShadowTexture = new ExternalTexture(defaultArctapShadow, "Note");
            heightIndicatorTexture = new ExternalTexture(defaultHeightIndicator, "Note");
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

            await arcTexture.Load();
            await arcTextureHighlight.Load();
            await arctapShadowTexture.Load();
            await heightIndicatorTexture.Load();

            traceMaterial.mainTexture = arcTexture.Value;
            baseArcMaterial.mainTexture = arcTexture.Value;
            baseArcHighlightMaterial.mainTexture = arcTextureHighlight.Value;

            foreach (var mat in arcMaterials)
            {
                mat.mainTexture = arcTexture.Value;
            }

            foreach (var mat in arcHighlightMaterials)
            {
                mat.mainTexture = arcTextureHighlight.Value;
            }

            Services.Render.SetTextures(heightIndicatorTexture.Value, arctapShadowTexture.Value);
            ResetTraceColors();
            ResetArcColors();
            ResetShadowColor();
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

            Destroy(baseArcMaterial);
            Destroy(baseArcHighlightMaterial);
            Destroy(traceMaterial);
            Destroy(shadowMaterial);

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

            arcTexture.Unload();
            arcTextureHighlight.Unload();
            arctapShadowTexture.Unload();
            heightIndicatorTexture.Unload();
        }

        private void ReloadNoteSkin(int inputMode)
        {
            Services.Chart?.ReloadSkin();
        }
    }
}