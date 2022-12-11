using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolService : MonoBehaviour, IScenecontrolService
    {
        private List<ScenecontrolEvent> events = new List<ScenecontrolEvent>();

        public List<ScenecontrolEvent> Events => events;

        public void Load(List<ScenecontrolEvent> cameras)
        {
            events = cameras;
            RebuildList();
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
        }

        private void RebuildList()
        {
            events.Sort((a, b) => a.Timing.CompareTo(b.Timing));
        }
    }
}