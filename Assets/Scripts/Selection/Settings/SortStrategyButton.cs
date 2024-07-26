using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    [RequireComponent(typeof(Button))]
    public class SortStrategyButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(CycleSortStrategy);
            I18n.OnLocaleChanged += OnLocale;
            OnLocale();
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(CycleSortStrategy);
            I18n.OnLocaleChanged -= OnLocale;
        }

        private void OnLocale()
        {
            text.text = I18n.S($"Gameplay.Selection.Sort.{Settings.SelectionSortStrategy.Value}");
        }

        private void CycleSortStrategy()
        {
            string newStrat = null;
            switch (Settings.SelectionSortStrategy.Value)
            {
                case SortByTitle.Typename:
                    newStrat = SortByComposer.Typename;
                    break;
                case SortByComposer.Typename:
                    newStrat = SortByDifficulty.Typename;
                    break;
                case SortByDifficulty.Typename:
                    newStrat = SortByCharter.Typename;
                    break;
                case SortByCharter.Typename:
                    newStrat = SortByAddedDate.Typename;
                    break;
                case SortByAddedDate.Typename:
                    newStrat = SortByGrade.Typename;
                    break;
                case SortByGrade.Typename:
                    newStrat = SortByScore.Typename;
                    break;
                case SortByScore.Typename:
                    newStrat = SortByPlayCount.Typename;
                    break;
                case SortByPlayCount.Typename:
                    newStrat = SortByRating.Typename;
                    break;
                case SortByRating.Typename:
                    newStrat = SortByTitle.Typename;
                    break;
                default:
                    newStrat = SortByTitle.Typename;
                    break;
            }

            Settings.SelectionSortStrategy.Value = newStrat;
            text.text = I18n.S($"Gameplay.Selection.Sort.{newStrat}");
        }
    }

}