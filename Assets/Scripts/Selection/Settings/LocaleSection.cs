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
            string[] locales = I18n.ListAllLocales();
            List<CellData> cellDatas = new List<CellData>();

            foreach (string locale in locales)
            {
                cellDatas.Add(new LocaleCellData()
                {
                    Pool = cellPool,
                    Size = localeCellSize,
                    Name = locale,
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