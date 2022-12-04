using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Particle
{
    public class ParticleService : MonoBehaviour, IParticleService
    {
        public const string Early = "EARLY";
        public const string Late = "LATE";

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
        [SerializeField] private int textParticlePoolCount = 50;
        [SerializeField] private float tapParticleLength = 0.3f;
        [SerializeField] private float textParticleLength = 0.5f;
        [SerializeField] private float earlyLateLength = 0.5f;
        [SerializeField] private float earlyLateFromY;
        [SerializeField] private float earlyLateToY;

        private Pool<Particle> tapParticlePool;
        private Pool<Particle> textParticlePool;
        private float lastEarlyLateRealTime = float.MinValue;

        private readonly Queue<ParticleSchedule> playingTapParticleQueue
            = new Queue<ParticleSchedule>();

        private readonly Queue<ParticleSchedule> playingTextParticleQueue
            = new Queue<ParticleSchedule>();

        public void UpdateParticles()
        {
            float currentRealTime = Time.realtimeSinceStartup;
            while (playingTapParticleQueue.Count > 0)
            {
                ParticleSchedule ps = playingTapParticleQueue.Peek();
                if (currentRealTime >= ps.ExpireAt)
                {
                    ps.Particle.Stop();
                    tapParticlePool.Return(ps.Particle);
                    playingTapParticleQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }

            while (playingTextParticleQueue.Count > 0)
            {
                ParticleSchedule ps = playingTextParticleQueue.Peek();
                if (currentRealTime >= ps.ExpireAt)
                {
                    ps.Particle.Stop();
                    textParticlePool.Return(ps.Particle);
                    playingTextParticleQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }

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
            ps.Play();
            playingTapParticleQueue.Enqueue(new ParticleSchedule()
            {
                ExpireAt = Time.realtimeSinceStartup + tapParticleLength,
                Particle = ps,
            });
        }

        public void PlayTextParticle(Vector3 worldPosition, JudgementResult result)
        {
            Material mat = pureMaterial;
            worldPosition.y = worldPosition.y + Values.TextParticleYOffset;

            if (result.IsPure())
            {
                mat = pureMaterial;
            }
            else if (result.IsFar())
            {
                mat = farMaterial;
            }
            else
            {
                mat = lostMaterial;
            }

            Vector2 screenPos = ConvertToScreen(worldPosition);
            Particle ps = textParticlePool.Get();
            ps.transform.localPosition = screenPos;
            ps.ApplyMaterial(mat);
            ps.Play();

            playingTextParticleQueue.Enqueue(new ParticleSchedule()
            {
                ExpireAt = Time.realtimeSinceStartup + textParticleLength,
                Particle = ps,
            });

            if (result.IsEarly())
            {
                earlyLateText.SetText(Early);
                earlyLateText.color = earlyColor;
                lastEarlyLateRealTime = Time.realtimeSinceStartup;
            }
            else if (result.IsLate())
            {
                earlyLateText.SetText(Late);
                earlyLateText.color = lateColor;
                lastEarlyLateRealTime = Time.realtimeSinceStartup;
            }
        }

        public void SetTapParticleSkin(Texture particleTexture)
        {
            tapParticlePrefab.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture = particleTexture;
            Pools.Destroy<Particle>("TapParticle");
            Pools.New<Particle>("TapParticle", tapParticlePrefab, tapParticleParent, tapParticlePoolCount);
        }

        private void Awake()
        {
            Pools.New<Particle>("TapParticle", tapParticlePrefab, tapParticleParent, tapParticlePoolCount);
            tapParticlePool = Pools.Get<Particle>("TapParticle");
            Pools.New<Particle>("TextParticle", textParticlePrefab, textParticleParent, textParticlePoolCount);
            textParticlePool = Pools.Get<Particle>("TextParticle");
        }

        private void OnDestroy()
        {
            Pools.Destroy<Particle>("TapParticle");
            Pools.Destroy<Particle>("TextParticle");
        }

        private Vector2 ConvertToScreen(Vector3 world)
        {
            return gameplayCamera.WorldToScreenPoint(world);
        }
    }
}