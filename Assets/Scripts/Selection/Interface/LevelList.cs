using System.Collections.Generic;
using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class LevelList : MonoBehaviour
    {
        [SerializeField] private StorageData storageData;
        [SerializeField] private InfiniteScroll scroll;
        [SerializeField] private GameObject levelCellPrefab;
        [SerializeField] private GameObject difficultyCellPrefab;
        private Pool<Cell> levelCellPool;
        private PackStorage selectedPack;
        private LevelStorage selectedLevel;
        private ChartSettings selectedChart;

        public PackStorage SelectedPack
        {
            get => selectedPack;
            set
            {
                selectedPack = value;
                RebuildList();
            }
        }

        public LevelStorage SelectedLevel
        {
            get => selectedLevel;
            set
            {
                bool shouldRebuild = true;
                foreach (var chart in value.Settings.Charts)
                {
                    if (chart.IsSameDifficulty(SelectedChart))
                    {
                        shouldRebuild = true;
                        break;
                    }
                }

                selectedLevel = value;
                if (shouldRebuild)
                {
                    RebuildList();
                }
            }
        }

        public ChartSettings SelectedChart
        {
            get => selectedChart;
            set
            {
                selectedChart = value;
                RebuildList();
            }
        }

        private void Awake()
        {
            levelCellPool = Pools.New<Cell>("LevelCell", levelCellPrefab, scroll.transform, 5);
            Pools.New<Image>("DifficultyCell", difficultyCellPrefab, transform, 30);
            storageData.OnStorageChange += RebuildList;
        }

        private void OnDestroy()
        {
            Pools.Destroy<Cell>("LevelCell");
            Pools.Destroy<Image>("DifficultyCell");
            storageData.OnStorageChange -= RebuildList;
        }

        private void RebuildList()
        {
            List<CellData> data = new List<CellData>();
            foreach (var level in selectedPack?.Levels ?? storageData.GetAllLevels())
            {
                data.Add(new LevelCellData
                {
                    Pool = levelCellPool,
                    Size = levelCellPrefab.GetComponent<RectTransform>().rect.height,
                    Children = null,
                    LevelList = this,
                    LevelStorage = level,
                });
            }

            scroll.SetData(data);
        }
    }
}