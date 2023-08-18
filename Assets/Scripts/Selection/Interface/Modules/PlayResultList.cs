using System.Collections.Generic;
using ArcCreate.Data;
using ArcCreate.Storage.Data;
using UnityEngine;

namespace ArcCreate.Selection.Interface
{
    public class PlayResultList : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private GameObject itemPrefab;
        private readonly List<PlayResultItem> items = new List<PlayResultItem>();

        public void Display(LevelStorage level, ChartSettings chart, IEnumerable<PlayResult> plays)
        {
            foreach (var item in items)
            {
                Destroy(item.gameObject);
            }

            items.Clear();

            foreach (var play in plays)
            {
                GameObject go = Instantiate(itemPrefab, parent);
                PlayResultItem item = go.GetComponent<PlayResultItem>();
                item.Display(level, chart, play);
                items.Add(item);
            }
        }
    }
}