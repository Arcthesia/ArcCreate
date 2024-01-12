using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Compose.Selection
{
    public struct NoteRaycastHit
    {
        public Vector3 HitPoint;
        public float HitDistance;
        public Note Note;
    }
}