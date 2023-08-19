using System.Collections.Generic;
using ArcCreate.Utility.InfiniteScroll;
using UnityEngine;

namespace ArcCreate.Selection.Interface
{
    public class LocaleSection : MonoBehaviour
    {
        [SerializeField] private InfiniteScroll scroll;
        [SerializeField] private GameObject localeCellPrefab;
        [SerializeField] private float localeCellSize;
        private Pool<Cell> cellPool;

        private void Awake()
        {
            cellPool = Pools.New<Cell>("LocaleCellPool", localeCellPrefab, scroll.transform, 6);
            List<I18n.LocaleEntry> locales = I18n.LocaleList;
            List<CellData> cellDatas = new List<CellData>();

            foreach (var locale in locales)
            {
                cellDatas.Add(new LocaleCellData()
                {
                    Pool = cellPool,
                    Size = localeCellSize,
                    Id = locale.Id,
                    Name = locale.LocalName,
                });
            }

            scroll.SetData(cellDatas);
        }

        private void OnDestroy()
        {
            Pools.Destroy<Cell>("LocaleCellPool");
        }
    }
}