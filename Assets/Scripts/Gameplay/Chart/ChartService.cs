using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    public class ChartService : MonoBehaviour, IChartService, IChartControl
    {
        [SerializeField] private GameplayData gameplayData;

        [SerializeField] private GameObject beatlinePrefab;
        [SerializeField] private Transform beatlineParent;
        [SerializeField] private Color beatlineColor;
        [SerializeField] private int beatlineCapacity;

        private readonly List<TimingGroup> timingGroups = new List<TimingGroup>();
        private BeatlineDisplay beatlineDisplay;

        public bool IsLoaded { get; private set; }

        public bool EnableColliderGeneration
        {
            get => Values.EnableColliderGeneration;
            set => Values.EnableColliderGeneration = value;
        }

        public bool EnableArcRebuildSegment
        {
            get => Values.EnableArcRebuildSegment;
            set => Values.EnableArcRebuildSegment = value;
        }

        public List<TimingGroup> TimingGroups => timingGroups;

        public void ReloadSkin()
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.ReloadSkin();
            }
        }

        public void ResetJudge()
        {
            int currentCombo = 0;
            int timing = Services.Audio.ChartTiming;
            int totalCombo = 0;
            InputMode inputMode = (InputMode)Settings.InputMode.Value;
            bool isAuto = inputMode == InputMode.Auto || inputMode == InputMode.AutoController;

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.ResetJudgeTo(timing);
                if (isAuto)
                {
                    currentCombo += tg.ComboAt(Services.Audio.ChartTiming);
                }

                totalCombo += tg.TotalCombo();
            }

            Services.Score.ResetScoreTo(currentCombo, totalCombo);
            Services.Judgement.ResetJudge();
            Services.Scenecontrol.UpdateScenecontrol(timing);
            Services.Camera.UpdateCamera(timing);
        }

        public IEnumerable<T> FindByTiming<T>(int from, int to)
            where T : ArcEvent
        {
            if (typeof(T) == typeof(ScenecontrolEvent))
            {
                foreach (var note in Services.Scenecontrol.FindByTiming(from, to))
                {
                    yield return note as T;
                }
            }

            if (typeof(T) == typeof(CameraEvent))
            {
                foreach (var note in Services.Camera.FindByTiming(from, to))
                {
                    yield return note as T;
                }
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                IEnumerable<T> groupNotes = tg.FindByTiming<T>(from, to);
                foreach (T note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public IEnumerable<T> FindByEndTiming<T>(int from, int to)
            where T : LongNote
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                IEnumerable<T> groupNotes = tg.FindByEndTiming<T>(from, to);
                foreach (T note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public IEnumerable<T> FindEventsWithinRange<T>(int from, int to)
            where T : ArcEvent
        {
            if (typeof(T) == typeof(ScenecontrolEvent))
            {
                foreach (var note in Services.Scenecontrol.FindWithinRange(from, to))
                {
                    yield return note as T;
                }
            }

            if (typeof(T) == typeof(CameraEvent))
            {
                foreach (var note in Services.Camera.FindWithinRange(from, to))
                {
                    yield return note as T;
                }
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                IEnumerable<T> groupNotes = tg.FindEventsWithinRange<T>(from, to);
                foreach (T note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public IEnumerable<T> GetAll<T>()
            where T : ArcEvent
        {
            if (typeof(T) == typeof(ScenecontrolEvent))
            {
                foreach (var note in Services.Scenecontrol.Events)
                {
                    yield return note as T;
                }
            }

            if (typeof(T) == typeof(CameraEvent))
            {
                foreach (var note in Services.Camera.Events)
                {
                    yield return note as T;
                }
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                IEnumerable<T> groupNotes = tg.GetEventType<T>();
                foreach (T note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public IEnumerable<Note> GetRenderingNotes()
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                if (!tg.GroupProperties.Visible || !tg.IsVisible)
                {
                    continue;
                }

                IEnumerable<Note> groupNotes = tg.GetRenderingNotes();

                foreach (Note note in groupNotes)
                {
                    yield return note;
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.Clear();
            }

            timingGroups.Clear();
            Services.Camera.Clear();
            Services.Scenecontrol.Clear();
        }

        public void LoadChart(ChartReader reader)
        {
            LoadChart(new ArcChart(reader));
            IsLoaded = true;
            Services.Audio.AudioTiming = 0;
        }

        public void LoadChart(ArcChart chart)
        {
            Clear();

            for (int j = 0; j < chart.TimingGroups.Count; j++)
            {
                ChartTimingGroup tg = chart.TimingGroups[j];
                TimingGroup newTg = new TimingGroup(j);
                timingGroups.Add(newTg);
                newTg.Load(tg);
            }

            Services.Camera.Load(chart.Cameras);
            Services.Scenecontrol.Load(chart.SceneControls);

            gameplayData.AudioOffset.Value = chart.AudioOffset;
            gameplayData.TimingPointDensityFactor.Value = chart.TimingPointDensity;

            ResetJudge();
        }

        public void ReloadBeatline(int audioLength)
        {
            beatlineDisplay.LoadFromTimingGroup(0, audioLength);
        }

        public void ReloadBeatline()
        {
            int length = 0;
            if (gameplayData.AudioClip.Value != null)
            {
                length = Mathf.RoundToInt(gameplayData.AudioClip.Value.length * 1000);
            }

            ReloadBeatline(length);
        }

        public void AddEvents(IEnumerable<ArcEvent> e)
        {
            foreach (var n in e)
            {
                if (n.TimingGroup >= timingGroups.Count)
                {
                    GetTimingGroup(n.TimingGroup);
                }
            }

            IEnumerable<CameraEvent> cameraEvents = e.Where(n => n is CameraEvent).Cast<CameraEvent>();
            IEnumerable<ScenecontrolEvent> scEvents = e.Where(n => n is ScenecontrolEvent).Cast<ScenecontrolEvent>();

            if (cameraEvents.Any())
            {
                Services.Camera.Add(cameraEvents);
                gameplayData.NotifyChartCameraEdit();
            }

            if (scEvents.Any())
            {
                Services.Scenecontrol.Add(scEvents);
                gameplayData.NotifyChartScenecontrolEdit();
            }

            if (e.Any(n => n is TimingEvent))
            {
                gameplayData.NotifyChartTimingEdit();
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.AddEvents(e.Where(n => n.TimingGroup == tg.GroupNumber));
            }

            gameplayData.NotifyChartEdit();
        }

        public void RemoveEvents(IEnumerable<ArcEvent> e)
        {
            IEnumerable<CameraEvent> cameraEvents = e.Where(n => n is CameraEvent).Cast<CameraEvent>();
            IEnumerable<ScenecontrolEvent> scEvents = e.Where(n => n is ScenecontrolEvent).Cast<ScenecontrolEvent>();

            if (cameraEvents.Any())
            {
                Services.Camera.Remove(cameraEvents);
                gameplayData.NotifyChartCameraEdit();
            }

            if (scEvents.Any())
            {
                Services.Scenecontrol.Remove(scEvents);
                gameplayData.NotifyChartScenecontrolEdit();
            }

            if (e.Any(n => n is TimingEvent))
            {
                gameplayData.NotifyChartTimingEdit();
            }

            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.RemoveEvents(e.Where(n => n.TimingGroup == tg.GroupNumber));
            }

            gameplayData.NotifyChartEdit();
        }

        public void UpdateEvents(IEnumerable<ArcEvent> e)
        {
            IEnumerable<CameraEvent> cameraEvents = e.Where(n => n is CameraEvent).Cast<CameraEvent>();
            IEnumerable<ScenecontrolEvent> scEvents = e.Where(n => n is ScenecontrolEvent).Cast<ScenecontrolEvent>();

            if (cameraEvents.Any())
            {
                Services.Camera.Change(cameraEvents);
                gameplayData.NotifyChartCameraEdit();
            }

            if (scEvents.Any())
            {
                Services.Scenecontrol.Change(scEvents);
                gameplayData.NotifyChartScenecontrolEdit();
            }

            if (e.Any(n => n is TimingEvent))
            {
                gameplayData.NotifyChartTimingEdit();
            }

            List<ArcEvent> tgChanged = e.Where(n => n.TimingGroupChanged).ToList();
            if (tgChanged.Count > 0)
            {
                tgChanged.Sort((a, b) => a.TimingGroupChangedFrom == b.TimingGroupChangedFrom
                                    ? a.TimingGroup.CompareTo(b.TimingGroup)
                                    : a.TimingGroupChangedFrom.CompareTo(b.TimingGroupChangedFrom));

                int from = tgChanged[0].TimingGroupChangedFrom;
                int to = tgChanged[0].TimingGroup;

                List<ArcEvent> currentTgChange = new List<ArcEvent>();
                for (int i = 0; i < tgChanged.Count; i++)
                {
                    ArcEvent n = tgChanged[i];
                    if (from == n.TimingGroupChangedFrom && to == n.TimingGroup)
                    {
                        currentTgChange.Add(n);
                    }
                    else
                    {
                        GetTimingGroup(from).RemoveEvents(currentTgChange);
                        TimingGroup target = GetTimingGroup(to);
                        foreach (var note in currentTgChange)
                        {
                            note.TimingGroup = target.GroupNumber;
                        }

                        target.AddEvents(currentTgChange);

                        from = n.TimingGroupChangedFrom;
                        to = n.TimingGroup;
                        currentTgChange.Clear();
                        currentTgChange.Add(n);
                    }

                    n.ResetTimingGroupChangedFrom();
                }

                {
                    GetTimingGroup(from).RemoveEvents(currentTgChange);
                    TimingGroup target = GetTimingGroup(to);
                    foreach (var note in currentTgChange)
                    {
                        note.TimingGroup = target.GroupNumber;
                        note.ResetTimingGroupChangedFrom();
                    }

                    target.AddEvents(currentTgChange);
                }
            }

            List<ArcEvent> tgUnchanged = e.Where(n => !n.TimingGroupChanged).ToList();
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.UpdateEvents(tgUnchanged.Where(n => n.TimingGroup == tg.GroupNumber));
            }

            gameplayData.NotifyChartEdit();
        }

        public TimingGroup GetTimingGroup(int tg)
        {
            if (tg < 0)
            {
                return timingGroups[0];
            }

            if (tg >= timingGroups.Count)
            {
                TimingGroup newTg = new TimingGroup(timingGroups.Count);
                newTg.Load();
                timingGroups.Add(newTg);
                if (string.IsNullOrEmpty(newTg.GroupProperties.FileName))
                {
                    newTg.GroupProperties.FileName = timingGroups[0].GroupProperties.FileName;
                }

                return newTg;
            }

            return timingGroups[tg];
        }

        public void RemoveTimingGroup(TimingGroup group)
        {
            group.Clear();
            timingGroups.Remove(group);
        }

        public void UpdateChartJudgement(int currentTiming)
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.UpdateGroupJudgement(currentTiming);
            }
        }

        public void UpdateChartRender(int currentTiming)
        {
            for (int i = 0; i < timingGroups.Count; i++)
            {
                TimingGroup tg = timingGroups[i];
                tg.UpdateGroupRender(currentTiming);

                if (i == 0)
                {
                    beatlineDisplay.UpdateBeatlines(tg.GetFloorPosition(currentTiming));
                }
            }
        }

        private void Awake()
        {
            var beatlinePool = Pools.New<BeatlineBehaviour>(Values.BeatlinePoolName, beatlinePrefab, beatlineParent, beatlineCapacity);

            Settings.GlobalAudioOffset.OnValueChanged.AddListener(OnGlobalOffsetChange);
            gameplayData.BaseBpm.OnValueChange += OnBaseBpm;
            gameplayData.TimingPointDensityFactor.OnValueChange += OnTimingPointDensityFactor;
            gameplayData.AudioOffset.OnValueChange += OnChartAudioOffset;
            gameplayData.AudioClip.OnValueChange += OnAudioClipChange;
            beatlineDisplay = new BeatlineDisplay(new GameplayBeatlineGenerator(beatlineColor), beatlinePool);
        }

        private void OnDestroy()
        {
            Pools.Destroy<BeatlineBehaviour>(Values.BeatlinePoolName);

            Settings.GlobalAudioOffset.OnValueChanged.RemoveListener(OnGlobalOffsetChange);
            gameplayData.BaseBpm.OnValueChange -= OnBaseBpm;
            gameplayData.TimingPointDensityFactor.OnValueChange -= OnTimingPointDensityFactor;
            gameplayData.AudioOffset.OnValueChange -= OnChartAudioOffset;
            gameplayData.AudioClip.OnValueChange -= OnAudioClipChange;
        }

        private void OnAudioClipChange(AudioClip obj)
        {
            ReloadBeatline(Mathf.RoundToInt(obj.length * 1000));
        }

        private void OnTimingPointDensityFactor(float value)
        {
            Values.TimingPointDensity = value;
            ResetJudge();
        }

        private void OnBaseBpm(float value)
        {
            Values.BaseBpm = value;
            ResetJudge();
            UpdateArcColliderMesh();
        }

        private void OnChartAudioOffset(int value)
        {
            Values.ChartAudioOffset = value;
            ResetJudge();
        }

        private void OnGlobalOffsetChange(int offset)
        {
            ResetJudge();
        }

        private void UpdateArcColliderMesh()
        {
            if (EnableColliderGeneration)
            {
                for (int i = 0; i < timingGroups.Count; i++)
                {
                    TimingGroup tg = timingGroups[i];
                    tg.BuildArcColliders();
                }
            }
            else
            {
                for (int i = 0; i < timingGroups.Count; i++)
                {
                    TimingGroup tg = timingGroups[i];
                    tg.CleanArcColliders();
                }
            }
        }
    }
}