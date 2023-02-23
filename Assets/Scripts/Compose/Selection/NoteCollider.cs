using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Compose.Selection
{
    public class NoteCollider : MonoBehaviour
    {
        private MeshCollider meshCollider;

        public Note Note { get; private set; }

        public void AssignNote(Note note, int timing)
        {
            Note = note;
            meshCollider.sharedMesh = note.GetColliderMesh();
            note.GetColliderPosition(timing, out Vector3 pos, out Vector3 scl);
            transform.localPosition = pos;
            transform.localScale = scl;
        }

        private void Awake()
        {
            meshCollider = GetComponent<MeshCollider>();
        }
    }
}