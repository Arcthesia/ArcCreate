using System;
using System.Collections.Generic;
using System.Linq;
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
        private static bool lastWasInPackList = true;

        [SerializeField] private StorageData storageData;
        [SerializeField] private InfiniteScroll scroll;
        [SerializeField] private GameObject packCellPrefab;
        [SerializeField] private float packCellSize;
        [SerializeField] private float autoScrollDuration = 0.3f;
        [SerializeField] private ScriptedAnimator packListAnimator;
        [SerializeField] private ScriptedAnimator levelListAnimator;
        [SerializeField] private ScriptedAnimator hideUIAnimator;
        [SerializeField] private Transform listButtonsParent;
        [SerializeField] private Transform packButtonsParent;
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
            packButtonsParent.SetAsLastSibling();
            packListCanvasGroup.interactable = true;
            packListCanvasGroup.blocksRaycasts = true;
            lastWasInPackList = true;
        }

        private void Awake()
        {
            packCellPool = Pools.New<Cell>("PackCell", packCellPrefab, scroll.transform, 5);

            storageData.OnStorageChange += RebuildList;
            storageData.OnSwitchToGameplayScene += HideUI;
            storageData.SelectedPack.OnValueChange += OnSelectedPack;
            backToPackListButton.onClick.AddListener(BackToPackList);
            allSongsPack.onClick.AddListener(SelectAllSongsPack);
            remotePack.onClick.AddListener(SwitchToRemoteScene);
            loadChartsPack.onClick.AddListener(OpenChartPicker);
            storageData.OnSwitchToGameplaySceneException += OnGameplayException;

            if (storageData.IsLoaded)
            {
                RebuildList();
            }

            if (lastWasInPackList)
            {
                StartupAnimation().Forget();
            }
            else
            {
                OnSelectedPack(storageData.SelectedPack.Value);
            }
        }

        private async UniTask StartupAnimation()
        {
            Settings.SelectionSortPackStrategy.OnValueChanged.AddListener(OnSortChange);
            hideUIAnimator.HideImmediate();
            packListAnimator.HideImmediate();
            await UniTask.DelayFrame(2);
            Services.Select.ClearSelection();
            packListAnimator.Show();
            hideUIAnimator.Show();
            levelListAnimator.HideImmediate();
            packListCanvasGroup.interactable = true;
            packListCanvasGroup.blocksRaycasts = true;
            lastWasInPackList = true;
        }

        private void OnDestroy()
        {
            Pools.Destroy<Cell>("PackCell");

            Settings.SelectionSortPackStrategy.OnValueChanged.RemoveListener(OnSortChange);
            storageData.OnStorageChange -= RebuildList;
            storageData.OnSwitchToGameplayScene -= HideUI;
            storageData.SelectedPack.OnValueChange -= OnSelectedPack;
            backToPackListButton.onClick.RemoveListener(BackToPackList);
            allSongsPack.onClick.RemoveListener(SelectAllSongsPack);
            remotePack.onClick.RemoveListener(SwitchToRemoteScene);
            loadChartsPack.onClick.RemoveListener(OpenChartPicker);
            storageData.OnSwitchToGameplaySceneException -= OnGameplayException;
        }

        private void OnSortChange(string arg0) => RebuildList();

        private void SelectAllSongsPack()
        {
            storageData.SelectedPack.Value = null;
        }

        private void OnGameplayException(Exception e)
        {
            ShowUI();
        }

        private void ShowUI()
        {
            if (lastWasInPackList)
            {
                packListAnimator.Show();
            }
            else
            {
                levelListAnimator.Show();
            }

            hideUIAnimator.Show();
            new TransitionSequence()
                .AddTransition(new TriangleTileTransition())
                .Show().Forget();
        }

        private void HideUI()
        {
            if (packListAnimator.IsShown)
            {
                packListAnimator.Hide();
            }

            if (levelListAnimator.IsShown)
            {
                levelListAnimator.Hide();
            }

            hideUIAnimator.Hide();
        }

        private void SwitchToRemoteScene()
        {
            Services.Select.ClearSelection();
            HideUI();
            TransitionSequence sequence = new TransitionSequence()
                .OnBoth()
                .AddTransition(new TriangleTileTransition())
                .AddTransition(new DecorationTransition());
            SceneTransitionManager.Instance.SetTransition(sequence);
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
            listButtonsParent.SetAsLastSibling();
            packListCanvasGroup.interactable = false;
            packListCanvasGroup.blocksRaycasts = false;
            lastWasInPackList = false;
        }

        private void RebuildList()
        {
            List<PackStorage> packs = storageData.GetAllPacks().ToList();
            ISortPackStrategy sortPack = GetSortPackStrategy(Settings.SelectionSortPackStrategy.Value);
            List<PackCellData> data = new List<PackCellData>();

            foreach (var pack in packs)
            {
                data.Add(new PackCellData
                {
                    PackStorage = pack,
                    Pool = packCellPool,
                    Size = packCellSize,
                });
            }

            scroll.SetData(sortPack.Sort(data).ToList<CellData>());
            FocusOnPack(storageData.SelectedPack.Value);
        }

        private ISortPackStrategy GetSortPackStrategy(string value)
        {
            switch(value)
            {
                case SortPackByName.Typename:
                    return new SortPackByName();
                case SortPackByPublisher.Typename:
                    return new SortPackByPublisher();
                case SortPackByAddedDate.Typename:
                    return new SortPackByAddedDate();
                default:
                    return new SortPackByName();
            }
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