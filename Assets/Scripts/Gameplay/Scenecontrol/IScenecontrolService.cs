using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using TMPro;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public interface IScenecontrolService : IScenecontrolControl
    {
        /// <summary>
        /// Gets the list of all scenecontrol events.
        /// </summary>
        List<ScenecontrolEvent> Events { get; }

        /// <summary>
        /// Load a list of scenecontrol events.
        /// </summary>
        /// <param name="scenecontrols">The list of events.</param>
        void Load(List<ScenecontrolEvent> scenecontrols);

        /// <summary>
        /// Update the scenecontrol state.
        /// </summary>
        /// <param name="currentTiming">The current audio timing value.</param>
        void UpdateScenecontrol(int currentTiming);

        /// <summary>
        /// Find all scenecontrol events with matching timing.
        /// </summary>
        /// <param name="from">The query timing value range's lower end.</param>
        /// <param name="to">The query timing value range's upper end.</param>
        /// <returns>All matching scenecontrol events.</returns>
        IEnumerable<ScenecontrolEvent> FindByTiming(int from, int to);

        /// <summary>
        /// Find all scenecontrol events bounded by provided timing range.
        /// </summary>
        /// <param name="from">The timing range's lower bound.</param>
        /// <param name="to">The timing range's upper bound.</param>
        /// <returns>All matching scenecontrol events.</returns>
        IEnumerable<ScenecontrolEvent> FindWithinRange(int from, int to);

        /// <summary>
        /// Add a collection of scenecontrol events.
        /// </summary>
        /// <param name="events">The events collection.</param>
        void Add(IEnumerable<ScenecontrolEvent> events);

        /// <summary>
        /// Remove a collection of scenecontrol events.
        /// </summary>
        /// <param name="events">The events collection.</param>
        void Remove(IEnumerable<ScenecontrolEvent> events);

        /// <summary>
        /// Notify a collection of scenecontrol events have had their values changed.
        /// </summary>
        /// <param name="events">The events collection.</param>
        void Change(IEnumerable<ScenecontrolEvent> events);

        /// <summary>
        /// Clear all events.
        /// </summary>
        void Clear();

        void AddReferencedController(ISceneController c);

        TMP_FontAsset GetFont(string font);
    }
}