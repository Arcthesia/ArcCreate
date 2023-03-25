using System.Collections.Generic;
using ArcCreate.SceneTransition;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Animation;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class PackList : MonoBehaviour
    {
        [SerializeField] private StorageData storageData;
        [SerializeField] private InfiniteScroll scroll;
        [SerializeField] private GameObject packCellPrefab;
        [SerializeField] private float packCellSize;
        [SerializeField] private float autoScrollDuration = 0.3f;
        [SerializeField] private ScriptedAnimator packListAnimator;
        [SerializeField] private ScriptedAnimator levelListAnimator;
        [SerializeField] private CanvasGroup packListCanvasGroup;
        [SerializeField] private Button backToPackListButton;
        [SerializeField] private Button allSongsPack;
        [SerializeField] private Button remotePack;
        [SerializeField] private Button loadChartsPack;
        private Pool<Cell> packCellPool;
        private Tween scrollTween;

        public void BackToPackList()
        {
            Services.Select.ClearSelection();
            packListAnimator.Show();
            levelListAnimator.Hide();
            packListCanvasGroup.interactable = true;
            packListCanvasGroup.blocksRaycasts = true;
        }

        private void Awake()
        {
            packCellPool = Pools.New<Cell>("PackCell", packCellPrefab, scroll.transform, 5);

            storageData.OnStorageChange += RebuildList;
            storageData.SelectedPack.OnValueChange += OnSelectedPack;
            backToPackListButton.onClick.AddListener(BackToPackList);
            allSongsPack.onClick.AddListener(SelectAllSongsPack);
            remotePack.onClick.AddListener(SwitchToRemoteScene);
            loadChartsPack.onClick.AddListener(OpenChartPicker);

            if (storageData.IsLoaded)
            {
                RebuildList();
            }

            BackToPackList();
        }

        private void OnDestroy()
        {
            Pools.Destroy<Cell>("PackCell");

            storageData.OnStorageChange -= RebuildList;
            storageData.SelectedPack.OnValueChange -= OnSelectedPack;
            backToPackListButton.onClick.RemoveListener(BackToPackList);
            allSongsPack.onClick.RemoveListener(SelectAllSongsPack);
            remotePack.onClick.RemoveListener(SwitchToRemoteScene);
            loadChartsPack.onClick.RemoveListener(OpenChartPicker);
        }

        private void SelectAllSongsPack()
        {
            storageData.SelectedPack.Value = null;
        }

        private void SwitchToRemoteScene()
        {
            Services.Select.ClearSelection();
            SceneTransitionManager.Instance.SetTransition(new ShutterTransition(1000));
            SceneTransitionManager.Instance.SwitchScene(SceneNames.RemoteScene).Forget();
        }

        private void OpenChartPicker()
        {
            storageData.NotifyOpenFilePicker();
        }

        private void OnSelectedPack(PackStorage pack)
        {
            if (pack?.Levels?.Count == 0)
            {
                return;
            }

            Services.Select.ClearSelection();
            packListAnimator.Hide();
            levelListAnimator.Show();
            packListCanvasGroup.interactable = false;
            packListCanvasGroup.blocksRaycasts = false;
        }

        private void RebuildList()
        {
            IEnumerable<PackStorage> packs = storageData.GetAllPacks();
            List<CellData> data = new List<CellData>();

            foreach (var pack in packs)
            {
                data.Add(new PackCellData
                {
                    PackStorage = pack,
                    Pool = packCellPool,
                    Size = packCellSize,
                });
            }

            scroll.SetData(data);
            FocusOnPack(storageData.SelectedPack.Value);
        }

        private void FocusOnPack(PackStorage pack)
        {
            float scrollFrom = scroll.Value;
            float scrollTo = 0;
            if (pack != null)
            {
                for (int i = 0; i < scroll.Data.Count; i++)
                {
                    CellData data = scroll.Data[i];
                    if (data is PackCellData packCell && packCell.PackStorage.Id == pack.Id)
                    {
                        scrollTo = scroll.Hierarchy[i].ValueToCenterCell;
                        break;
                    }
                }

                scrollTween?.Kill();
                scrollTween = DOTween.To((float val) => scroll.Value = val, scrollFrom, scrollTo, autoScrollDuration).SetEase(Ease.OutExpo);
            }
        }
    }
}