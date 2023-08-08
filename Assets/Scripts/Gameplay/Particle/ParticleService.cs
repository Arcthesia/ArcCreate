using System.Collections.Generic;
using ArcCreate.Data;
using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Utility.Extension;
using ArcCreate.Utility.ExternalAssets;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Particle
{
    public class ParticleService : MonoBehaviour, IParticleService
    {
        [SerializeField] private Camera gameplayCamera;

        [Header("Text particle")]
        [SerializeField] private GameObject textParticlePrefab;
        [SerializeField] private Material perfectMaterial;
        [SerializeField] private Material goodMaterial;
        [SerializeField] private Material missMaterial;

        [Header("Early late display")]
        [SerializeField] private TMP_Text earlyLateText;
        [SerializeField] private RectTransform earlyLateTransform;
        [SerializeField] private Color earlyColor;
        [SerializeField] private Color lateColor;

        [Header("Other particles")]
        [SerializeField] private GameObject tapParticlePrefab;
        [SerializeField] private GameObject arcNoteParticlePrefab;
        [SerializeField] private GameObject holdNoteParticlePrefab;

        [Header("Parents")]
        [SerializeField] private Transform tapParticleParent;
        [SerializeField] private Transform textParticleParent;
        [SerializeField] private Transform longNoteParticleParent;
        [SerializeField] private RectTransform screenParticlesRect;

        [Header("Numbers")]
        [SerializeField] private int tapParticlePoolCount = 50;
        [SerializeField] private int textParticlePoolCount = 200;
        [SerializeField] private int arcParticlePoolCount = 4;
        [SerializeField] private int holdParticlePoolCount = 6;
        [SerializeField] private float earlyLateLength = 0.5f;
        [SerializeField] private float earlyLateFromY;
        [SerializeField] private float earlyLateToY;
        [SerializeField] private float longParticlePersistDuration;

        private ExternalTexture perfectMaterialTexture;
        private ExternalTexture goodMaterialTexture;
        private ExternalTexture missMaterialTexture;
        private ExternalTexture arcParticleTexture;
        private ExternalTexture holdParticleTexture;

        private ParticlePool<Particle> tapParticlePool;
        private ParticlePool<Particle> textParticlePool;
        private Pool<Particle> arcParticlePool;
        private Pool<Particle> holdParticlePool;
        private float lastEarlyLateRealTime = float.MinValue;
        private readonly char[] offsetCharArray = new char[8];

        private readonly Dictionary<LongNote, ParticleSchedule> playingArcParticles
            = new Dictionary<LongNote, ParticleSchedule>(4);

        private readonly Dictionary<LongNote, ParticleSchedule> playingHoldParticles
            = new Dictionary<LongNote, ParticleSchedule>(6);

        private readonly List<LongNote> arcParticlesToPrune = new List<LongNote>();
        private readonly List<LongNote> holdParticlesToPrune = new List<LongNote>();

        private Material PerfectMaterial { get; set; }

        private Material GoodMaterial { get; set; }

        private Material MissMaterial { get; set; }

        public void UpdateParticles()
        {
            float currentRealTime = Time.time;

            if (currentRealTime <= lastEarlyLateRealTime + earlyLateLength)
            {
                float x = 1 - ((currentRealTime - lastEarlyLateRealTime) / earlyLateLength);
                float easeCubicOut = 1 - (x * x * x);
                float y = earlyLateFromY + ((earlyLateToY - earlyLateFromY) * easeCubicOut);
                float a = 1 - easeCubicOut;

                earlyLateTransform.anchoredPosition = new Vector3(0, y, 0);

                Color c = earlyLateText.color;
                c.a = a;
                earlyLateText.color = c;
            }

            arcParticlesToPrune.Clear();
            foreach (var pair in playingArcParticles)
            {
                LongNote reference = pair.Key;
                ParticleSchedule schedule = pair.Value;
                if (currentRealTime >= schedule.ExpireAt)
                {
                    schedule.Particle.Stop();
                    arcParticlePool.Return(schedule.Particle);
                    arcParticlesToPrune.Add(reference);
                }
            }

            for (int i = 0; i < arcParticlesToPrune.Count; i++)
            {
                LongNote reference = arcParticlesToPrune[i];
                playingArcParticles.Remove(reference);
            }

            holdParticlesToPrune.Clear();
            foreach (var pair in playingHoldParticles)
            {
                LongNote reference = pair.Key;
                ParticleSchedule schedule = pair.Value;
                if (currentRealTime >= schedule.ExpireAt)
                {
                    schedule.Particle.Stop();
                    holdParticlePool.Return(schedule.Particle);
                    holdParticlesToPrune.Add(reference);
                }
            }

            for (int i = 0; i < holdParticlesToPrune.Count; i++)
            {
                LongNote reference = holdParticlesToPrune[i];
                playingHoldParticles.Remove(reference);
            }
        }

        public void PlayTapParticle(Vector3 worldPosition, JudgementResult result)
        {
            if (result.IsMiss())
            {
                return;
            }

            Vector2 screenPos = ConvertToScreen(worldPosition);
            Particle ps = tapParticlePool.Get();
            ps.transform.localPosition = screenPos;
            ps.Stop();
            ps.Play();
        }

        public void PlayTextParticle(Vector3 worldPosition, JudgementResult result, Option<int> offset)
        {
            Material mat = PerfectMaterial;

            if ((!Settings.ShowMaxJudgement.Value && result.IsMax())
             || (!Settings.ShowPerfectJudgement.Value && result.IsPerfect() && !result.IsMax())
             || (!Settings.ShowGoodJudgement.Value && result.IsGood())
             || (!Settings.ShowMissJudgement.Value && result.IsMiss()))
            {
                return;
            }

            if (result.IsPerfect())
            {
                mat = PerfectMaterial;
            }
            else if (result.IsGood())
            {
                mat = GoodMaterial;
            }
            else
            {
                mat = MissMaterial;
            }

            Vector2 screenPos = ConvertToScreen(worldPosition);
            screenPos.y += Values.TextParticleYOffset;
            Particle ps = textParticlePool.Get();
            ps.transform.localPosition = screenPos;
            ps.Stop();
            ps.ApplyMaterial(mat);
            ps.Play();

            if (result.IsEarly() && (result.IsGood() || Settings.ShowEarlyLatePerfect.Value))
            {
                if (Settings.DisplayMsDifference.Value && offset.HasValue)
                {
                    SetOffsetNumber(offsetCharArray, offset.Value, out int length);
                    earlyLateText.SetCharArray(offsetCharArray, offsetCharArray.Length - length, length);
                }
                else
                {
                    earlyLateText.SetText(Values.EarlyText);
                }

                earlyLateText.color = earlyColor;
                lastEarlyLateRealTime = Time.time;
            }
            else if (result.IsLate() && (result.IsGood() || Settings.ShowEarlyLatePerfect.Value))
            {
                if (Settings.DisplayMsDifference.Value && offset.HasValue)
                {
                    SetOffsetNumber(offsetCharArray, offset.Value, out int length);
                    earlyLateText.SetCharArray(offsetCharArray, offsetCharArray.Length - length, length);
                }
                else
                {
                    earlyLateText.SetText(Values.LateText);
                }

                earlyLateText.color = lateColor;
                lastEarlyLateRealTime = Time.time;
            }
        }

        public void PlayHoldParticle(LongNote reference, Vector3 worldPosition)
        {
            float currentRealTime = Time.time;
            Vector2 screenPos = ConvertToScreen(worldPosition);

            if (!playingHoldParticles.ContainsKey(reference))
            {
                Particle ps = holdParticlePool.Get();
                ps.transform.localPosition = screenPos;
                ps.Play();

                playingHoldParticles.Add(
                    reference,
                    new ParticleSchedule()
                    {
                        ExpireAt = currentRealTime + longParticlePersistDuration,
                        Particle = ps,
                    });
            }
            else
            {
                ParticleSchedule ps = playingHoldParticles[reference];
                ps.Particle.transform.localPosition = screenPos;
                playingHoldParticles[reference] = new ParticleSchedule()
                {
                    ExpireAt = currentRealTime + longParticlePersistDuration,
                    Particle = ps.Particle,
                };
            }
        }

        public void PlayArcParticle(int colorId, LongNote reference, Vector3 worldPosition)
        {
            var (color1, color2) = Services.Skin.GetArcParticleColor(colorId);
            float currentRealTime = Time.time;
            Vector2 screenPos = ConvertToScreen(worldPosition);

            if (!playingArcParticles.ContainsKey(reference))
            {
                Particle ps = arcParticlePool.Get();
                ps.ApplyColor(color1, color2);
                ps.transform.localPosition = screenPos;
                ps.Play();

                playingArcParticles.Add(
                    reference,
                    new ParticleSchedule()
                    {
                        ExpireAt = currentRealTime + longParticlePersistDuration,
                        Particle = ps,
                    });
            }
            else
            {
                ParticleSchedule ps = playingArcParticles[reference];
                ps.Particle.ApplyColor(color1, color2);
                ps.Particle.transform.localPosition = screenPos;
                playingArcParticles[reference] = new ParticleSchedule()
                {
                    ExpireAt = currentRealTime + longParticlePersistDuration,
                    Particle = ps.Particle,
                };
            }
        }

        public void SetTapParticleSkin(Texture particleTexture)
        {
            tapParticlePrefab.GetComponent<ParticleSystemRenderer>().material.mainTexture = particleTexture;
            Pools.Destroy<Particle>(Values.TapParticlePoolName);
            tapParticlePool.Destroy();
            tapParticlePool = new ParticlePool<Particle>(
                tapParticlePrefab,
                tapParticleParent,
                tapParticlePoolCount);
        }

        public void SetHoldParticleSkin(Color colorMin, Color colorMax, Gradient fromGradient, Gradient toGradient, Color colorGrid)
        {
            ParticleSystem pts = holdNoteParticlePrefab.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule module = pts.main;
            ParticleSystem.ColorOverLifetimeModule colorModule = pts.colorOverLifetime;

            module.startColor = new ParticleSystem.MinMaxGradient(colorMin, colorMax);
            colorModule.color = new ParticleSystem.MinMaxGradient(fromGradient, toGradient);
            pts.GetComponentInChildren<SpriteRenderer>().color = colorGrid;

            Pools.Destroy<Particle>(Values.HoldParticlePoolName);
            holdParticlePool = Pools.New<Particle>(
                Values.HoldParticlePoolName,
                holdNoteParticlePrefab,
                longNoteParticleParent,
                holdParticlePoolCount);

            playingHoldParticles.Clear();
        }

        private void Awake()
        {
            tapParticlePrefab = Instantiate(tapParticlePrefab, transform);
            arcNoteParticlePrefab = Instantiate(arcNoteParticlePrefab, transform);
            holdNoteParticlePrefab = Instantiate(holdNoteParticlePrefab, transform);
            PerfectMaterial = Instantiate(perfectMaterial);
            GoodMaterial = Instantiate(goodMaterial);
            MissMaterial = Instantiate(missMaterial);

            tapParticlePool = new ParticlePool<Particle>(
                tapParticlePrefab,
                tapParticleParent,
                tapParticlePoolCount);

            textParticlePool = new ParticlePool<Particle>(
                textParticlePrefab,
                textParticleParent,
                textParticlePoolCount);

            arcParticlePool = Pools.New<Particle>(
                Values.ArcParticlePoolName,
                arcNoteParticlePrefab,
                longNoteParticleParent,
                arcParticlePoolCount);

            holdParticlePool = Pools.New<Particle>(
                Values.HoldParticlePoolName,
                holdNoteParticlePrefab,
                longNoteParticleParent,
                holdParticlePoolCount);

            perfectMaterialTexture = new ExternalTexture(PerfectMaterial.mainTexture, "Particles");
            goodMaterialTexture = new ExternalTexture(GoodMaterial.mainTexture, "Particles");
            missMaterialTexture = new ExternalTexture(MissMaterial.mainTexture, "Particles");

            var particlePrefabRenderer = arcNoteParticlePrefab.GetComponent<ParticleSystemRenderer>();
            arcParticleTexture = new ExternalTexture(particlePrefabRenderer.material.mainTexture, "Particles");

            particlePrefabRenderer = holdNoteParticlePrefab.GetComponent<ParticleSystemRenderer>();
            holdParticleTexture = new ExternalTexture(particlePrefabRenderer.material.mainTexture, "Particles");

            Settings.LateEarlyTextPosition.OnValueChanged.AddListener(OnLateEarlyPositionSettings);
            OnLateEarlyPositionSettings(Settings.LateEarlyTextPosition.Value);

            LoadExternalParticleSkin().Forget();
        }

        private void OnLateEarlyPositionSettings(int val)
        {
            EarlyLateTextPosition pos = (EarlyLateTextPosition)val;
            float yAnchor = 0.5f;
            switch (pos)
            {
                case EarlyLateTextPosition.Top:
                    yAnchor = 0.9f;
                    break;
                case EarlyLateTextPosition.Bottom:
                    yAnchor = 0.25f;
                    break;
                case EarlyLateTextPosition.Middle:
                default:
                    yAnchor = 0.5f;
                    break;
            }

            earlyLateTransform.anchorMin = new Vector2(earlyLateTransform.anchorMin.x, yAnchor);
            earlyLateTransform.anchorMax = new Vector2(earlyLateTransform.anchorMax.x, yAnchor);
        }

        private async UniTask LoadExternalParticleSkin()
        {
            await perfectMaterialTexture.Load();
            await goodMaterialTexture.Load();
            await missMaterialTexture.Load();
            await arcParticleTexture.Load();

            PerfectMaterial.mainTexture = perfectMaterialTexture.Value;
            GoodMaterial.mainTexture = goodMaterialTexture.Value;
            MissMaterial.mainTexture = missMaterialTexture.Value;

            var particlePrefabRenderer = arcNoteParticlePrefab.GetComponent<ParticleSystemRenderer>();
            particlePrefabRenderer.material.mainTexture = arcParticleTexture.Value;

            particlePrefabRenderer = holdNoteParticlePrefab.GetComponent<ParticleSystemRenderer>();
            particlePrefabRenderer.material.mainTexture = holdParticleTexture.Value;
        }

        private void OnDestroy()
        {
            tapParticlePool.Destroy();
            textParticlePool.Destroy();
            Pools.Destroy<Particle>(Values.ArcParticlePoolName);

            perfectMaterialTexture.Unload();
            goodMaterialTexture.Unload();
            missMaterialTexture.Unload();
            arcParticleTexture.Unload();

            Destroy(PerfectMaterial);
            Destroy(GoodMaterial);
            Destroy(MissMaterial);

            Settings.LateEarlyTextPosition.OnValueChanged.RemoveListener(OnLateEarlyPositionSettings);
        }

        private Vector2 ConvertToScreen(Vector3 world)
        {
            Vector2 viewport = gameplayCamera.WorldToViewportPoint(world);
            return new Vector2(viewport.x * screenParticlesRect.rect.width, viewport.y * screenParticlesRect.rect.height);
        }

        private void SetOffsetNumber(char[] charArray, int value, out int length)
        {
            charArray.SetNumberDigitsToArray(value, out length);
            if (value >= 0)
            {
                length++;
                charArray[charArray.Length - length] = '+';
            }

            for (int i = 3; i < charArray.Length; i++)
            {
                charArray[i - 3] = charArray[i];
            }

            charArray[charArray.Length - 3] = ' ';
            charArray[charArray.Length - 2] = 'm';
            charArray[charArray.Length - 1] = 's';
            length += 3;
        }
    }
}