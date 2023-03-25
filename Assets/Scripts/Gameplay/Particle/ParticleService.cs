using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Gameplay.Skin;
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
        [SerializeField] private Material pureMaterial;
        [SerializeField] private Material farMaterial;
        [SerializeField] private Material lostMaterial;

        [Header("Early late display")]
        [SerializeField] private TMP_Text earlyLateText;
        [SerializeField] private RectTransform earlyLateTransform;
        [SerializeField] private Color earlyColor;
        [SerializeField] private Color lateColor;

        [Header("Other particles")]
        [SerializeField] private GameObject tapParticlePrefab;
        [SerializeField] private GameObject longNoteParticlePrefab;

        [Header("Parents")]
        [SerializeField] private Transform tapParticleParent;
        [SerializeField] private Transform textParticleParent;
        [SerializeField] private Transform longNoteParticleParent;

        [Header("Numbers")]
        [SerializeField] private int tapParticlePoolCount = 50;
        [SerializeField] private int textParticlePoolCount = 200;
        [SerializeField] private int longParticlePoolCount = 10;
        [SerializeField] private float earlyLateLength = 0.5f;
        [SerializeField] private float earlyLateFromY;
        [SerializeField] private float earlyLateToY;
        [SerializeField] private float longParticlePersistDuration;

        private ExternalTexture pureMaterialTexture;
        private ExternalTexture farMaterialTexture;
        private ExternalTexture lostMaterialTexture;
        private ExternalTexture longParticleTexture;

        private ParticlePool<Particle> tapParticlePool;
        private ParticlePool<Particle> textParticlePool;
        private Pool<Particle> longParticlePool;
        private float lastEarlyLateRealTime = float.MinValue;

        private readonly Dictionary<LongNote, ParticleSchedule> playingLongParticles
            = new Dictionary<LongNote, ParticleSchedule>(10);

        private readonly List<LongNote> longParticlesToPrune = new List<LongNote>();

        private Material PureMaterial { get; set; }

        private Material FarMaterial { get; set; }

        private Material LostMaterial { get; set; }

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

            longParticlesToPrune.Clear();
            foreach (var pair in playingLongParticles)
            {
                LongNote reference = pair.Key;
                ParticleSchedule schedule = pair.Value;
                if (currentRealTime >= schedule.ExpireAt)
                {
                    schedule.Particle.Stop();
                    longParticlePool.Return(schedule.Particle);
                    longParticlesToPrune.Add(reference);
                }
            }

            for (int i = 0; i < longParticlesToPrune.Count; i++)
            {
                LongNote reference = longParticlesToPrune[i];
                playingLongParticles.Remove(reference);
            }
        }

        public void PlayTapParticle(Vector3 worldPosition, JudgementResult result)
        {
            if (result.IsLost())
            {
                return;
            }

            Vector2 screenPos = ConvertToScreen(worldPosition);
            Particle ps = tapParticlePool.Get();
            ps.transform.localPosition = screenPos;
            ps.Stop();
            ps.Play();
        }

        public void PlayTextParticle(Vector3 worldPosition, JudgementResult result)
        {
            Material mat = PureMaterial;

            if (result.IsPure())
            {
                mat = PureMaterial;
            }
            else if (result.IsFar())
            {
                mat = FarMaterial;
            }
            else
            {
                mat = LostMaterial;
            }

            Vector2 screenPos = ConvertToScreen(worldPosition);
            screenPos.y += Values.TextParticleYOffset;
            Particle ps = textParticlePool.Get();
            ps.transform.localPosition = screenPos;
            ps.Stop();
            ps.ApplyMaterial(mat);
            ps.Play();

            if (result.IsEarly())
            {
                earlyLateText.SetText(Values.EarlyText);
                earlyLateText.color = earlyColor;
                lastEarlyLateRealTime = Time.time;
            }
            else if (result.IsLate())
            {
                earlyLateText.SetText(Values.LateText);
                earlyLateText.color = lateColor;
                lastEarlyLateRealTime = Time.time;
            }
        }

        public void PlayLongParticle(LongNote reference, Vector3 worldPosition)
        {
            float currentRealTime = Time.time;
            Vector2 screenPos = ConvertToScreen(worldPosition);

            if (!playingLongParticles.ContainsKey(reference))
            {
                Particle ps = longParticlePool.Get();
                ps.transform.localPosition = screenPos;
                ps.Play();

                playingLongParticles.Add(
                    reference,
                    new ParticleSchedule()
                    {
                        ExpireAt = currentRealTime + longParticlePersistDuration,
                        Particle = ps,
                    });
            }
            else
            {
                ParticleSchedule ps = playingLongParticles[reference];
                ps.Particle.transform.localPosition = screenPos;
                playingLongParticles[reference] = new ParticleSchedule()
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

        public void SetLongParticleSkin(Color colorMin, Color colorMax, Gradient fromGradient, Gradient toGradient)
        {
            ParticleSystem pts = longNoteParticlePrefab.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule module = pts.main;
            ParticleSystem.ColorOverLifetimeModule colorModule = pts.colorOverLifetime;

            module.startColor = new ParticleSystem.MinMaxGradient(colorMin, colorMax);
            colorModule.color = new ParticleSystem.MinMaxGradient(fromGradient, toGradient);

            Pools.Destroy<Particle>(Values.LongParticlePoolName);
            longParticlePool = Pools.New<Particle>(
                Values.LongParticlePoolName,
                longNoteParticlePrefab,
                longNoteParticleParent,
                longParticlePoolCount);

            playingLongParticles.Clear();
        }

        private void Awake()
        {
            tapParticlePrefab = Instantiate(tapParticlePrefab, transform);
            longNoteParticlePrefab = Instantiate(longNoteParticlePrefab, transform);
            PureMaterial = Instantiate(pureMaterial);
            FarMaterial = Instantiate(farMaterial);
            LostMaterial = Instantiate(lostMaterial);

            tapParticlePool = new ParticlePool<Particle>(
                tapParticlePrefab,
                tapParticleParent,
                tapParticlePoolCount);

            textParticlePool = new ParticlePool<Particle>(
                textParticlePrefab,
                textParticleParent,
                textParticlePoolCount);

            longParticlePool = Pools.New<Particle>(
                Values.LongParticlePoolName,
                longNoteParticlePrefab,
                longNoteParticleParent,
                longParticlePoolCount);

            pureMaterialTexture = new ExternalTexture(PureMaterial.mainTexture, "Particles");
            farMaterialTexture = new ExternalTexture(FarMaterial.mainTexture, "Particles");
            lostMaterialTexture = new ExternalTexture(LostMaterial.mainTexture, "Particles");

            var particlePrefabRenderer = longNoteParticlePrefab.GetComponent<ParticleSystemRenderer>();
            longParticleTexture = new ExternalTexture(particlePrefabRenderer.material.mainTexture, "Particles");

            LoadExternalParticleSkin().Forget();
        }

        private async UniTask LoadExternalParticleSkin()
        {
            await pureMaterialTexture.Load();
            await farMaterialTexture.Load();
            await lostMaterialTexture.Load();
            await longParticleTexture.Load();

            PureMaterial.mainTexture = pureMaterialTexture.Value;
            FarMaterial.mainTexture = farMaterialTexture.Value;
            LostMaterial.mainTexture = lostMaterialTexture.Value;

            var particlePrefabRenderer = longNoteParticlePrefab.GetComponent<ParticleSystemRenderer>();
            particlePrefabRenderer.material.mainTexture = longParticleTexture.Value;
        }

        private void OnDestroy()
        {
            tapParticlePool.Destroy();
            textParticlePool.Destroy();
            Pools.Destroy<Particle>(Values.LongParticlePoolName);

            pureMaterialTexture.Unload();
            farMaterialTexture.Unload();
            lostMaterialTexture.Unload();
            longParticleTexture.Unload();

            Destroy(PureMaterial);
            Destroy(FarMaterial);
            Destroy(LostMaterial);
        }

        private Vector2 ConvertToScreen(Vector3 world)
        {
            Vector2 viewport = gameplayCamera.WorldToViewportPoint(world);
            return new Vector2(viewport.x * gameplayCamera.pixelWidth, viewport.y * gameplayCamera.pixelHeight);
        }
    }
}