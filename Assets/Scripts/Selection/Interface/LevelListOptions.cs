using System;
using System.Threading;
using ArcCreate.Utility.Animation;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class LevelListOptions : MonoBehaviour
    {
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private int debounceMs;
        [SerializeField] private Button toggleSearchButton;
        [SerializeField] private GameObject searchIcon;
        [SerializeField] private GameObject hideIcon;
        [SerializeField] private ScriptedAnimator showSearchAnimator;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public event Action OnNeedRebuild;

        public IGroupStrategy GroupStrategy { get; private set; }

        public ISortStrategy SortStrategy { get; private set; }

        public string SearchQuery { get; private set; }

        public void Setup()
        {
            Settings.SelectionGroupStrategy.OnValueChanged.AddListener(OnGroupStrategyChanged);
            Settings.SelectionSortStrategy.OnValueChanged.AddListener(OnSortStrategyChanged);
            SetGroupStrategy(Settings.SelectionGroupStrategy.Value);
            SetSortStrategy(Settings.SelectionSortStrategy.Value);

            toggleSearchButton.onClick.AddListener(ToggleSearchMenu);
            searchField.onValueChanged.AddListener(OnSearchField);
        }

        private void OnDestroy()
        {
            Settings.SelectionGroupStrategy.OnValueChanged.RemoveListener(OnGroupStrategyChanged);
            Settings.SelectionSortStrategy.OnValueChanged.RemoveListener(OnSortStrategyChanged);

            toggleSearchButton.onClick.RemoveListener(ToggleSearchMenu);
            searchField.onValueChanged.RemoveListener(OnSearchField);
        }

        private void OnSearchField(string query)
        {
            SearchQuery = query;
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            QueueRebuild(cts.Token).Forget();
        }

        private async UniTask QueueRebuild(CancellationToken ct)
        {
            bool cancelled = await UniTask.Delay(debounceMs, cancellationToken: ct).SuppressCancellationThrow();
            if (cancelled)
            {
                return;
            }

            OnNeedRebuild.Invoke();
        }

        private void ToggleSearchMenu()
        {
            string prevQuery = SearchQuery;
            if (showSearchAnimator.IsShown)
            {
                showSearchAnimator.Hide();
                SearchQuery = string.Empty;
                searchIcon.SetActive(true);
                hideIcon.SetActive(false);
            }
            else
            {
                showSearchAnimator.Show();
                SearchQuery = searchField.text;
                searchField.ActivateInputField();
                searchIcon.SetActive(false);
                hideIcon.SetActive(true);
            }

            if (prevQuery != SearchQuery)
            {
                OnNeedRebuild?.Invoke();
            }
        }

        private void OnGroupStrategyChanged(string strat)
        {
            SetGroupStrategy(strat);
            OnNeedRebuild?.Invoke();
        }

        private void SetGroupStrategy(string strat)
        {
            switch (strat)
            {
                case NoGroup.Typename:
                    GroupStrategy = new NoGroup();
                    break;
                case GroupByGrade.Typename:
                    GroupStrategy = new GroupByGrade();
                    break;
                case GroupByRank.Typename:
                    GroupStrategy = new GroupByRank();
                    break;
                case GroupByDifficulty.Typename:
                    GroupStrategy = new GroupByDifficulty();
                    break;
                case GroupByCharter.Typename:
                    GroupStrategy = new GroupByCharter();
                    break;
                case GroupByRating.Typename:
                    GroupStrategy = new GroupByRating();
                    break;
                default:
                    GroupStrategy = new NoGroup();
                    break;
            }
        }

        private void OnSortStrategyChanged(string strat)
        {
            SetSortStrategy(strat);
            OnNeedRebuild?.Invoke();
        }

        private void SetSortStrategy(string strat)
        {
            switch (strat)
            {
                case SortByAddedDate.Typename:
                    SortStrategy = new SortByAddedDate();
                    break;
                case SortByDifficulty.Typename:
                    SortStrategy = new SortByDifficulty();
                    break;
                case SortByGrade.Typename:
                    SortStrategy = new SortByGrade();
                    break;
                case SortByScore.Typename:
                    SortStrategy = new SortByScore();
                    break;
                case SortByTitle.Typename:
                    SortStrategy = new SortByTitle();
                    break;
                case SortByComposer.Typename:
                    SortStrategy = new SortByComposer();
                    break;
                case SortByCharter.Typename:
                    SortStrategy = new SortByCharter();
                    break;
                case SortByPlayCount.Typename:
                    SortStrategy = new SortByPlayCount();
                    break;
                case SortByRating.Typename:
                    SortStrategy = new SortByRating();
                    break;
                default:
                    SortStrategy = new SortByDifficulty();
                    break;
            }
        }
    }
}