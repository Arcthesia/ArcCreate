using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolService : MonoBehaviour, IScenecontrolService
    {
        private List<ScenecontrolEvent> events = new List<ScenecontrolEvent>();
        [SerializeField] private TMP_FontAsset defaultFont;
        [SerializeField] private Scene scene;
        [SerializeField] private HashSet<Controller> alwaysReferencedControllers = new HashSet<Controller>();
        private float count = 0;
        private int loopSwitch = 1;
        private readonly List<Controller> referencedControllers = new List<Controller>();

        public List<ScenecontrolEvent> Events => events;

        public Scene Scene => scene;

        public TMP_FontAsset DefaultFont => defaultFont;

        public List<Controller> ReferencedControllers => referencedControllers;

        public float CurrentSpeed { get; private set; }

        public float CurrentGlow { get; private set; }

        public string ScenecontrolFolder { get; set; }

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

            CurrentSpeed = Services.Audio.IsPlaying ? bpm / Values.BaseBpm : 0;
            CurrentGlow = count / beatDuration;

            foreach (Controller c in referencedControllers)
            {
                c.UpdateController(currentTiming);
            }
        }

        public void Clean()
        {
            referencedControllers.Clear();
            referencedControllers.AddRange(alwaysReferencedControllers);
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

        public void Import(string def)
        {
            var units = JsonConvert.DeserializeObject<List<SerializedUnit>>(def);
            var deserialization = new ScenecontrolDeserialization(scene, units);
            foreach (var unit in deserialization.Result)
            {
                if (unit is Controller c)
                {
                    referencedControllers.Add(c);
                }
            }
        }

        private void RebuildList()
        {
            events.Sort((a, b) => a.Timing.CompareTo(b.Timing));
        }

        private void Awake()
        {
            referencedControllers.AddRange(alwaysReferencedControllers);
            foreach (var c in scene.DisabledByDefault)
            {
                c.Start();
            }
        }
    }
}