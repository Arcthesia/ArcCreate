using System.Collections.Generic;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Compose.Timeline
{
    public class TicksDisplay : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private GameObject tickPrefab;
        [SerializeField] private RectTransform tickParent;
        [SerializeField] private int tickCapacity;
        [SerializeField] private float maxBpmToCalculate = 1000;
        [SerializeField] private float minDistBetweenTicks = 200;

        private readonly List<int> timingEventsTiming = new List<int>();
        private readonly List<int> beatsTiming = new List<int>();
        private readonly List<int> barsTiming = new List<int>();

        private Pool<Tick> tickPool;

        public void UpdateTicks()
        {
            tickPool.ReturnAll();
            if (TryDisplay(beatsTiming))
            {
                return;
            }

            int skip = 1;
            while (!TryDisplay(barsTiming, skip))
            {
                skip *= 4;
                if (skip >= 1024)
                {
                    TryDisplay(timingEventsTiming, 1, true);
                    return;
                }
            }
        }

        private bool TryDisplay(List<int> timings, int skip = 1, bool forced = false)
        {
            if (timings.Count == 0)
            {
                return false;
            }

            int viewFromTiming = Services.Timeline.ViewFromTiming;
            int viewToTiming = Services.Timeline.ViewToTiming;
            float width = tickParent.rect.width;
            int offset = gameplayData.AudioOffset.Value;

            int indexFrom = timings.BisectLeft(viewFromTiming);
            indexFrom = Mathf.FloorToInt(indexFrom / skip) * skip;

            int i = indexFrom;

            if (!forced)
            {
                float lastPos = float.MinValue;
                while (i < timings.Count && timings[i] + offset <= viewToTiming)
                {
                    float pos = (float)(timings[i] + offset - viewFromTiming) / (viewToTiming - viewFromTiming) * width;
                    if (Mathf.Abs(pos - lastPos) < minDistBetweenTicks)
                    {
                        return false;
                    }

                    i += skip;
                    lastPos = pos;
                }
            }

            i = indexFrom;
            while (i < timings.Count && timings[i] + offset <= viewToTiming)
            {
                float pos = (float)(timings[i] + offset - viewFromTiming) / (viewToTiming - viewFromTiming) * width;
                Tick tick = tickPool.Get();
                tick.SetTick(pos, timings[i]);
                i += skip;
            }

            return true;
        }

        private void GenerateTickData(int untilTiming)
        {
            TimingGroup tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
            List<TimingEvent> timings = tg.Timings;

            timingEventsTiming.Clear();
            beatsTiming.Clear();
            barsTiming.Clear();

            for (int i = 0; i < timings.Count - 1; i++)
            {
                TimingEvent curr = timings[i];
                TimingEvent next = timings[i + 1];

                timingEventsTiming.Add(curr.Timing);

                if (Mathf.Approximately(curr.Bpm, 0) || curr.Bpm > maxBpmToCalculate)
                {
                    continue;
                }

                float beatLength = 60000 / Mathf.Abs(curr.Bpm);
                float barLength = beatLength * curr.Divisor;

                for (float j = curr.Timing; j < next.Timing; j += beatLength)
                {
                    beatsTiming.Add(Mathf.RoundToInt(j));
                    if (barLength < beatLength)
                    {
                        barsTiming.Add(Mathf.RoundToInt(j));
                    }
                }

                if (barLength < beatLength)
                {
                    continue;
                }

                for (float j = curr.Timing; j < next.Timing; j += barLength)
                {
                    barsTiming.Add(Mathf.RoundToInt(j));
                }
            }

            {
                TimingEvent curr = timings[timings.Count - 1];

                timingEventsTiming.Add(curr.Timing);
                if (!Mathf.Approximately(curr.Bpm, 0) && curr.Bpm <= maxBpmToCalculate)
                {
                    float beatLength = 60000f / Mathf.Abs(curr.Bpm);
                    float barLength = beatLength * curr.Divisor;

                    for (float j = curr.Timing; j < untilTiming; j += beatLength)
                    {
                        beatsTiming.Add(Mathf.RoundToInt(j));
                        if (barLength < beatLength)
                        {
                            barsTiming.Add(Mathf.RoundToInt(j));
                        }
                    }

                    if (barLength >= beatLength)
                    {
                        for (float j = curr.Timing; j < untilTiming; j += barLength)
                        {
                            barsTiming.Add(Mathf.RoundToInt(j));
                        }
                    }
                }
            }

            UpdateTicks();
        }

        private void Awake()
        {
            tickPool = Pools.New<Tick>(Values.TickPoolName, tickPrefab, tickParent, tickCapacity);
            gameplayData.AudioClip.OnValueChange += OnAudioLoad;
            gameplayData.OnChartFileLoad += OnChartChange;
            gameplayData.OnChartTimingEdit += OnChartChange;
            gameplayData.AudioOffset.OnValueChange += OnChartChangeButCallbackHasAnInt;
            Values.EditingTimingGroup.OnValueChange += OnChartChangeButCallbackHasAnInt;
        }

        private void OnDestroy()
        {
            gameplayData.AudioClip.OnValueChange -= OnAudioLoad;
            gameplayData.OnChartFileLoad -= OnChartChange;
            gameplayData.OnChartTimingEdit -= OnChartChange;
            gameplayData.AudioOffset.OnValueChange -= OnChartChangeButCallbackHasAnInt;
            Values.EditingTimingGroup.OnValueChange -= OnChartChangeButCallbackHasAnInt;
        }

        private void OnAudioLoad(AudioClip clip)
        {
            if (clip != null)
            {
                GenerateTickData(Mathf.RoundToInt(clip.length * 1000));
            }
        }

        private void OnChartChange()
        {
            OnAudioLoad(gameplayData.AudioClip.Value);
        }

        private void OnChartChangeButCallbackHasAnInt(int notEvenUsed)
        {
            OnAudioLoad(gameplayData.AudioClip.Value);
        }
    }
}