using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public interface IGridService
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to enable the grid.
        /// </summary>
        bool IsGridEnabled { get; set; }

        /// <summary>
        /// Snap a timing value to a timing point on the timing grid if the grid is enabled,
        /// or returns the passed in value as-is otherwise.
        /// </summary>
        /// <param name="sourceTiming">The base timing point.</param>
        /// <returns>The closest timing point on the grid if grid is enabled, or <see cref="sourceTiming"/> otherwise.</returns>
        int SnapTimingToGridIfGridIsEnabled(int sourceTiming);

        /// <summary>
        /// Snap a timing value to a timing point on the timing grid.
        /// </summary>
        /// <param name="sourceTiming">The base timing point.</param>
        /// <returns>The closest timing point on the grid.</returns>
        int SnapTimingToGrid(int sourceTiming);

        /// <summary>
        /// Move backward on the timing grid from a timing point.
        /// </summary>
        /// <param name="sourceTiming">The base timing point.</param>
        /// <returns>The largest timing point on the grid that's smaller than the passed in value.</returns>
        int MoveTimingBackward(int sourceTiming);

        /// <summary>
        /// Move forward on the timing grid from a timing point.
        /// </summary>
        /// <param name="sourceTiming">The base timing point.</param>
        /// <returns>The smallest timing point on the grid that's larger than the passed in value.</returns>
        int MoveTimingForward(int sourceTiming);

        /// <summary>
        /// Move backward on the timing grid from a timing point to the nearest beat.
        /// </summary>
        /// <param name="sourceTiming">The base timing point.</param>
        /// <returns>The smallest timing point on the grid that's larger than the passed in value.</returns>
        int MoveTimingBackwardByBeat(int sourceTiming);

        /// <summary>
        /// Move forward on the timing grid from a timing point to the nearest beat.
        /// </summary>
        /// <param name="sourceTiming">The base timing point.</param>
        /// <returns>The smallest timing point on the grid that's larger than the passed in value.</returns>
        int MoveTimingForwardByBeat(int sourceTiming);

        /// <summary>
        /// Snap a point onto the vertical grid if the grid is enabled.
        /// or returns the passed in value as-is otherwise.
        /// </summary>
        /// <param name="point">The base point to snap.</param>
        /// <returns>The closest point on the vertical grid.</returns>
        Vector2 SnapPointToGridIfEnabled(Vector2 point);

        /// <summary>
        /// Snap a point onto the vertical grid.
        /// </summary>
        /// <param name="point">The base point to snap.</param>
        /// <returns>The closest point on the vertical grid.</returns>
        Vector2 SnapPointToGrid(Vector2 point);

        /// <summary>
        /// Get the bounds of the vertical grid.
        /// </summary>
        /// <returns>Coordinates of the bound.</returns>
        (float fromX, float fromY, float toX, float toY) GetVerticalGridBound();

        /// <summary>
        /// Get the horizontal bounds of the timing grid.
        /// </summary>
        /// <returns>Coordinates of the bound.</returns>
        (float fromX, float fromZ, float toX, float toZ) GetTimingGridBound();
    }
}