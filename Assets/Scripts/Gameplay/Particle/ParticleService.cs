using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Judgement;
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
        [SerializeField] private int textParticlePoolCount = 50;
        [SerializeField] private int longParticlePoolCount = 10;
        [SerializeField] private float tapParticleLength = 0.3f;
        [SerializeField] private float textParticleLength = 0.5f;
        [SerializeField] private float earlyLateLength = 0.5f;
        [SerializeField] private float earlyLateFromY;
        [SerializeField] private float earlyLateToY;
        [SerializeField] private float longParticlePersistDuration;

        private Pool<Particle> tapParticlePool;
        private Pool<Particle> textParticlePool;
        private Pool<Particle> longParticlePool;
        private float lastEarlyLateRealTime = float.MinValue;

        private readonly Queue<ParticleSchedule> playingTapParticleQueue
            = new Queue<ParticleSchedule>();

        private readonly Queue<ParticleSchedule> playingTextParticleQueue
            = new Queue<ParticleSchedule>();

        private readonly Dictionary<LongNote, ParticleSchedule> playingLongParticles
            = new Dictionary<LongNote, ParticleSchedule>(10);

        private readonly List<LongNote> longParticlesToPrune = new List<LongNote>();

        public void UpdateParticles()
        {
            float currentRealTime = Time.time;
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
            ps.Play();
            playingTapParticleQueue.Enqueue(new ParticleSchedule()
            {
                ExpireAt = Time.time + tapParticleLength,
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
                ExpireAt = Time.time + textParticleLength,
                Particle = ps,
            });

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
            tapParticlePool = Pools.New<Particle>(
                Values.TapParticlePoolName,
                tapParticlePrefab,
                tapParticleParent,
                tapParticlePoolCount);
        }

        public void SetLongParticleSkin(Color colorMin, Color colorMax)
        {
            var module = longNoteParticlePrefab.GetComponent<ParticleSystem>().main;
            module.startColor = new ParticleSystem.MinMaxGradient(colorMin, colorMax);
            Pools.Destroy<Particle>(Values.LongParticlePoolName);
            longParticlePool = Pools.New<Particle>(
                Values.LongParticlePoolName,
                longNoteParticlePrefab,
                longNoteParticleParent,
                longParticlePoolCount);
        }

        private void Awake()
        {
            tapParticlePool = Pools.New<Particle>(
                Values.TapParticlePoolName,
                tapParticlePrefab,
                tapParticleParent,
                tapParticlePoolCount);

            textParticlePool = Pools.New<Particle>(
                Values.TextParticlePoolName,
                textParticlePrefab,
                textParticleParent,
                textParticlePoolCount);

            longParticlePool = Pools.New<Particle>(
                Values.LongParticlePoolName,
                longNoteParticlePrefab,
                longNoteParticleParent,
                longParticlePoolCount);
        }

        private void OnDestroy()
        {
            Pools.Destroy<Particle>(Values.TapParticlePoolName);
            Pools.Destroy<Particle>(Values.TextParticlePoolName);
            Pools.Destroy<Particle>(Values.LongParticlePoolName);
        }

        private Vector2 ConvertToScreen(Vector3 world)
        {
            return gameplayCamera.WorldToScreenPoint(world);
        }
    }
}