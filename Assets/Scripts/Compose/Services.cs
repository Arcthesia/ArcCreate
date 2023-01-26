using ArcCreate.Compose.History;
using ArcCreate.Compose.Popups;
using ArcCreate.Compose.Project;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using UnityEngine;

namespace ArcCreate.Compose
{
    internal class Services : MonoBehaviour
    {
        [SerializeField] private ProjectService project;
        [SerializeField] private PopupsService popups;
        [SerializeField] private TimelineService timeline;
        [SerializeField] private HistoryService history;

        public static IProjectService Project { get; private set; }

        public static IPopupsService Popups { get; private set; }

        public static ITimelineService Timeline { get; private set; }

        public static IGameplayControl Gameplay { get; set; }

        public static IHistoryService History { get; set; }

        private void Awake()
        {
            Project = project;
            Popups = popups;
            Timeline = timeline;
            History = history;
        }
    }
}