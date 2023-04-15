using ArcCreate.Gameplay;
using UnityEngine;

namespace ArcCreate.Compose.Selection
{
    public class SelectMeshBuilder : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private GameObject colliderPrefab;
        [SerializeField] private Transform colliderParent;
        [SerializeField] private int capacity;
        private Pool<NoteCollider> colliderPool;
        private int lastUpdateAt = int.MinValue;

        public void RefreshCollider()
        {
            int timing = Services.Gameplay.Audio.ChartTiming;
            if (timing != lastUpdateAt)
            {
                colliderPool.ReturnAll();

                foreach (var note in Services.Gameplay.Chart.GetRenderingNotes())
                {
                    if (note.TimingGroupInstance.GroupProperties.Editable)
                    {
                        NoteCollider collider = colliderPool.Get();
                        collider.AssignNote(note, timing);
                    }
                }
            }

            lastUpdateAt = timing;
        }

        private void Awake()
        {
            colliderPool = Pools.New<NoteCollider>(Values.ColliderPoolName, colliderPrefab, colliderParent, capacity);
            gameplayData.OnChartEdit += SetDirty;
        }

        private void SetDirty()
        {
            lastUpdateAt = int.MinValue;
        }
    }
}