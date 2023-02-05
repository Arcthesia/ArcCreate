using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public abstract class NoteBehaviour : MonoBehaviour
    {
        public abstract Note Note { get; }

        public bool IsSelected
        {
            get => Note.IsSelected;
            set => Note.IsSelected = value;
        }
    }
}