using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolService : MonoBehaviour, IScenecontrolService
    {
        private List<ScenecontrolEvent> events = new List<ScenecontrolEvent>();
        [SerializeField] private TMP_FontAsset defaultFont;
        [SerializeField] private Scene scene;
        private float count = 0;
        private int loopSwitch = 1;
        private readonly List<Controller> controllers = new List<Controller>();

        public List<ScenecontrolEvent> Events => events;

        public TMP_FontAsset DefaultFont => defaultFont;

        public float CurrentSpeed { get; private set; }

        public float CurrentGlow { get; private set; }

        public string SceneControlFolder => "";

        public void Load(List<ScenecontrolEvent> cameras)
        {
            events = cameras;
            RebuildList();
        }

        public void Clear()
        {
            events.Clear();
            CleanControllers();
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

            foreach (Controller c in controllers)
            {
                c.UpdateController(currentTiming);
            }
        }

        public void RemoveController(Controller controller)
        {
            controllers.Remove(controller);
        }

        public void AddController(Controller controller)
        {
            controllers.Add(controller);
        }

        public void CleanControllers()
        {
            foreach (var c in controllers)
            {
                c.CleanController();
            }
        }

        private void RebuildList()
        {
            events.Sort((a, b) => a.Timing.CompareTo(b.Timing));
        }
    }
}