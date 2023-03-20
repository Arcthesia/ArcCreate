using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolService : MonoBehaviour, IScenecontrolService
    {
        private static readonly int OffsetShaderId = Shader.PropertyToID("_Offset");
        [SerializeField] private TMP_FontAsset defaultFont;
        [SerializeField] private List<FontEntry> fonts;
        [SerializeField] private Scene scene;
        [SerializeField] private PostProcessing postProcessing;
        [SerializeField] private SpriteRenderer trackSprite;
        [SerializeField] private SpriteRenderer singleLineL;
        [SerializeField] private SpriteRenderer singleLineR;
        [SerializeField] private GlowingSprite skyInputLine;
        [SerializeField] private GlowingSprite skyInputLabel;
        private List<ScenecontrolEvent> events = new List<ScenecontrolEvent>();
        private readonly List<ISceneController> referencedControllers = new List<ISceneController>();
        private float trackOffset = 0;
        private float singleLineOffset = 0;
        private float count = 0;
        private int loopSwitch = 1;

        public List<ScenecontrolEvent> Events => events;

        public Scene Scene => scene;

        public PostProcessing PostProcessing => postProcessing;

        public string ScenecontrolFolder { get; set; }

        public bool IsLoaded { get; private set; }

        public void Load(List<ScenecontrolEvent> cameras)
        {
            events = cameras;
            RebuildList();
        }

        public void Clear()
        {
            events.Clear();
            Clean();
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

        public IEnumerable<ScenecontrolEvent> FindByTiming(int from, int to)
        {
            int i = events.BisectLeft(from, n => n.Timing);
            while (i >= 0 && i < events.Count && events[i].Timing >= from && events[i].Timing <= to)
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

            float speed = Services.Audio.IsPlaying ? bpm / Values.BaseBpm : 0;
            float glowAlpha = Mathf.Lerp(0.75f, 1, count / beatDuration);

            foreach (var c in referencedControllers)
            {
                c.UpdateController(currentTiming);
            }

            trackOffset += Time.deltaTime * speed * 6;
            trackSprite.material.SetFloat(OffsetShaderId, trackOffset);
            singleLineOffset += (speed >= 0) ? (Time.deltaTime * speed * 6) : (Time.deltaTime * 0.6f);
            singleLineL.material.SetFloat(OffsetShaderId, singleLineOffset);
            singleLineR.material.SetFloat(OffsetShaderId, singleLineOffset);
            skyInputLine.ApplyGlow(glowAlpha);
            skyInputLabel.ApplyGlow(glowAlpha);

            Services.Score.ClearJudgementsThisFrame();
        }

        public void Clean()
        {
            Scene.ClearCache();
            foreach (var c in referencedControllers)
            {
                c.CleanController();
            }

            referencedControllers.Clear();
        }

        public string Export()
        {
            var serialization = new ScenecontrolSerialization();
            foreach (var c in referencedControllers)
            {
                serialization.AddUnitAndGetId(c);
            }

            return JsonConvert.SerializeObject(serialization.Result);
        }

        public void Import(string def, IFileAccessWrapper fileAccess)
        {
            scene.SetFileAccess(fileAccess);
            var units = JsonConvert.DeserializeObject<List<SerializedUnit>>(def);
            var deserialization = new ScenecontrolDeserialization(scene, postProcessing, units);
            foreach (var unit in deserialization.Result)
            {
                if (unit is Controller c)
                {
                    referencedControllers.Add(c);
                }
            }
        }

        public void AddReferencedController(ISceneController c)
        {
            if (!referencedControllers.Contains(c))
            {
                referencedControllers.Add(c);
            }
        }

        public void WaitForSceneLoad()
        {
            IsLoaded = false;
            scene.WaitForTasksComplete().ContinueWith(() => IsLoaded = true);
        }

        public TMP_FontAsset GetFont(string font)
        {
            foreach (var entry in fonts)
            {
                if (entry.Name == font || entry.FontAsset.name == font)
                {
                    return entry.FontAsset;
                }
            }

            return defaultFont;
        }

        private void RebuildList()
        {
            events.Sort((a, b) => a.Timing.CompareTo(b.Timing));
        }

        private void Awake()
        {
            foreach (var c in scene.DisabledByDefault)
            {
                c.Start();
            }
        }

        [System.Serializable]
        private struct FontEntry
        {
            public string Name;

            public TMP_FontAsset FontAsset;
        }
    }
}