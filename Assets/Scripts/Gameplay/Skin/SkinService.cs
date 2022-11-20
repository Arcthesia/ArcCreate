using System.Collections.Generic;
using System.Linq;
using ArcCreate.Gameplay.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ArcCreate.Gameplay.Skin
{
    public class SkinService : MonoBehaviour, ISkinService
    {
        [Header("Objects")]
        [SerializeField] private SpriteRenderer singleLineL;
        [SerializeField] private SpriteRenderer singleLineR;
        [SerializeField] private Text comboText;
        [SerializeField] private Outline comboOutline;
        [SerializeField] private SpriteRenderer trackExtraL;
        [SerializeField] private SpriteRenderer trackExtraR;
        [SerializeField] private SpriteRenderer track;
        [SerializeField] private SpriteRenderer trackExtraEdgeL;
        [SerializeField] private SpriteRenderer trackExtraEdgeR;
        [SerializeField] private SpriteRenderer[] criticalLines = new SpriteRenderer[4];
        [SerializeField] private SpriteSO jacketShadowSO;
        [SerializeField] private Image background;
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

        private NoteSkinOption currentNoteSkin;
        private Color holdEffectColorMin;
        private Color holdEffectColorMax;

        private int highColorShaderId;
        private int lowColorShaderId;
        private int shadowColorShaderId;

        public Sprite GetTapSkin(Tap note)
            => currentNoteSkin.GetTapSkin(note);

        public (Sprite normal, Sprite highlight) GetHoldSkin(Hold note)
            => currentNoteSkin.GetHoldSkin(note);

        public (Mesh mesh, Material material) GetArcTapSkin(ArcTap note)
            => currentNoteSkin.GetArcTapSkin(note);

        public (Material normal, Material highlight, Sprite arcCap, Color psMin, Color psMax) GetArcSkin(Arc note)
        {
            Sprite arcCap = currentNoteSkin.GetArcCapSprite();

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

        public void SetBackground(Sprite sprite)
        {
            background.sprite = sprite;
        }

        public void SetVideoBackground(string videoUrl)
        {
            videoBackground.url = videoUrl;
        }

        public void SetSkin(string alignment, string noteSkin, string particle, string track, string accent, string single)
        {
            AlignmentOption alignmentOpt = alignmentOptions.FirstOrDefault(opt => opt.Name == alignment);
            alignmentOpt = alignmentOpt != null ? alignmentOpt : alignmentOptions[0];

            ParticleSkinOption particleSkinOpt = particleSkinOptions.FirstOrDefault(opt => opt.Name == particle);
            TrackSkinOption trackSkinOpt = trackSkinOptions.FirstOrDefault(opt => opt.Name == track);
            AccentOption accentOpt = accentOptions.FirstOrDefault(opt => opt.Name == accent);
            SingleLineOption singleLineOpt = singleLineOptions.FirstOrDefault(opt => opt.Name == single);
            NoteSkinOption noteSkinOpt = noteSkinOptions.FirstOrDefault(opt => opt.Name == noteSkin);
            SetSkin(alignmentOpt, noteSkinOpt, particleSkinOpt, trackSkinOpt, accentOpt, singleLineOpt);
        }

        public void SetSkin(
            AlignmentOption alignment,
            NoteSkinOption noteSkin,
            ParticleSkinOption particle,
            TrackSkinOption track,
            AccentOption accent,
            SingleLineOption single)
        {
            jacketShadowSO.Value = alignment.JacketShadowSkin;

            // Fallback to default skin
            noteSkin = noteSkin != null ? noteSkin : alignment.DefaultNoteOption;
            particle = particle != null ? particle : alignment.DefaultParticleOption;
            track = track != null ? track : alignment.DefaultTrackOption;
            accent = accent != null ? accent : alignment.DefaultAccentOption;
            single = single != null ? single : alignment.DefaultSingleLineOption;

            currentNoteSkin = noteSkin;

            Services.Effect.SetParticleSkin(particle.ParticleSkin);
            holdEffectColorMax = particle.HoldEffectColorMax;
            holdEffectColorMin = particle.HoldEffectColorMin;

            this.track.sprite = track.TrackSkin;
            trackExtraEdgeL.sprite = track.TrackSkin;
            trackExtraEdgeR.sprite = track.TrackSkin;
            trackExtraL.sprite = track.TrackExtraSkin;
            trackExtraR.sprite = track.TrackExtraSkin;

            foreach (var c in criticalLines)
            {
                c.sprite = accent.CriticalLineSkin;
            }

            comboText.color = accent.ComboColor;
            comboOutline.effectColor = accent.ComboColor;

            singleLineL.sprite = single.SingleLineSkin;
            singleLineL.enabled = single.Enable;
            singleLineR.sprite = single.SingleLineSkin;
            singleLineR.enabled = single.Enable;
        }

        public void SetTraceColor(Color color, Color shadow)
        {
            traceMaterial.SetColor(highColorShaderId, color);
            traceMaterial.SetColor(lowColorShaderId, color);
            traceMaterial.SetColor(shadowColorShaderId, shadow);
        }

        public void SetArcColors(List<Color> arcs, List<Color> arcLows, Color shadow)
        {
            arcMaterials.ForEach(mat => Destroy(mat));
            arcHighlightMaterials.ForEach(mat => Destroy(mat));
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

        private void Awake()
        {
            highColorShaderId = Shader.PropertyToID("_Color");
            lowColorShaderId = Shader.PropertyToID("_LowColor");
            shadowColorShaderId = Shader.PropertyToID("_ShadowColor");
            Settings.InputMode.OnValueChanged.AddListener(ReloadNoteSkin);
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