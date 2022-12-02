using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public interface ILongNote<Behaviour> : INote<Behaviour>
        where Behaviour : MonoBehaviour
    {
        int EndTiming { get; }

        double EndFloorPosition { get; }
    }
}
