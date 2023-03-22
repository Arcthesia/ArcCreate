using ArcCreate.Selection.Select;
using UnityEngine;

namespace ArcCreate.Selection
{
    internal class Services : MonoBehaviour
    {
        [SerializeField] private SelectService select;

        public static ISelectService Select { get; set; }

        private void Awake()
        {
            Select = select;
        }
    }
}