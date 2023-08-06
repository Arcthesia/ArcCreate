using ArcCreate.Selection.Select;
using ArcCreate.Selection.SoundEffect;
using UnityEngine;

namespace ArcCreate.Selection
{
    internal class Services : MonoBehaviour
    {
        [SerializeField] private SelectService select;

        [SerializeField] private SoundEffectService soundEffect;

        public static ISelectService Select { get; set; }

        public static ISoundEffectService SoundEffect { get; set; }

        private void Awake()
        {
            Select = select;
            SoundEffect = soundEffect;
        }
    }
}