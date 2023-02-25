using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolService : MonoBehaviour, IScenecontrolService
    {
        private List<ScenecontrolEvent> events = new List<ScenecontrolEvent>();

        [SerializeField] private SpriteRenderer trackSpriteRenderer;
        [SerializeField] private SpriteRenderer singleLineLeftRenderer;
        [SerializeField] private SpriteRenderer singleLineRightRenderer;
        [SerializeField] private SpriteRenderer skyInputLineSpriteRenderer;
        private float trackOffset = 0;
        private float singleLineOffset = 0;
        private float count = 0;
        private int loopSwitch = 1;
        private readonly int offsetShaderId = Shader.PropertyToID("_Offset");

        public List<ScenecontrolEvent> Events => events;

        public void Load(List<ScenecontrolEvent> cameras)
        {
            events = cameras;
            RebuildList();
        }

        public void Clear()
        {
            events.Clear();
        }

        public void Add(IEnumerable<ScenecontrolEvent> events)
        {
            this.events.AddRange(events);
            RebuildList();
        }

        public void Change(IEnumerable<ScenecontrolEvent> events)
        {
            RebuildList();
        }

        public void Remove(IEnumerable<ScenecontrolEvent> events)
        {
            foreach (var sc in events)
            {
                this.events.Remove(sc);
            }

            RebuildList();
        }

        public IEnumerable<ScenecontrolEvent> FindByTiming(int timing)
        {
            int i = events.BisectLeft(timing, n => n.Timing);
            while (i >= 0 && i < events.Count && events[i].Timing == timing)
            {
                yield return events[i];
                i++;
            }
        }

        public IEnumerable<ScenecontrolEvent> FindWithinRange(int from, int to)
        {
            for (int i = 0; i < events.Count; i++)
            {
                ScenecontrolEvent sc = events[i];
                if (sc.Timing >= from && sc.Timing <= to)
                {
                    yield return sc;
                }
            }
        }

        public void UpdateScenecontrol(int currentTiming)
        {
            // TEMPORARY BEFORE A PROPER SCENECONTROL SYSTEM IS IMPLEMENTED
            float bpm = Services.Chart.GetTimingGroup(0).GetBpm(currentTiming);
            float beatDuration = (bpm != 0) ? 60.0f / bpm : Mathf.Infinity;

            bpm = Mathf.Abs(bpm);

            count += Time.deltaTime * loopSwitch;
            if (count >= beatDuration)
            {
                count = beatDuration;
                loopSwitch *= -1;
            }
            else if (count <= 0)
            {
                count = 0;
                loopSwitch *= -1;
            }

            float currentAlpha = Mathf.Lerp(0.75f, 1.0f, count / beatDuration);
            skyInputLineSpriteRenderer.color = new Color(1, 1, 1, currentAlpha);

            float speed = Services.Audio.IsPlaying ? bpm / Values.BaseBpm : 0;
            trackOffset += Time.deltaTime * speed * 6;
            singleLineOffset += (speed >= 0) ? (Time.deltaTime * speed * 6) : (Time.deltaTime * 0.6f);
            trackSpriteRenderer.sharedMaterial.SetFloat(offsetShaderId, trackOffset);
            singleLineLeftRenderer.sharedMaterial.SetFloat(offsetShaderId, singleLineOffset);
            singleLineRightRenderer.sharedMaterial.SetFloat(offsetShaderId, singleLineOffset);
        }

        private void RebuildList()
        {
            events.Sort((a, b) => a.Timing.CompareTo(b.Timing));
        }

        private void Awake()
        {
            trackSpriteRenderer.sharedMaterial = Instantiate(trackSpriteRenderer.sharedMaterial);
        }
    }
}