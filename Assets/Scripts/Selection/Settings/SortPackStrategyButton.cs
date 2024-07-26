using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    [RequireComponent(typeof(Button))]
    public class SortPackStrategyButton : MonoBehaviour
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
            text.text = I18n.S($"Gameplay.Selection.SortPack.{Settings.SelectionSortPackStrategy.Value}");
        }

        private void CycleSortStrategy()
        {
            string newStrat = null;
            switch (Settings.SelectionSortPackStrategy.Value)
            {
                case SortPackByName.Typename:
                    newStrat = SortPackByPublisher.Typename;
                    break;
                case SortPackByPublisher.Typename:
                    newStrat = SortPackByAddedDate.Typename;
                    break;
                case SortPackByAddedDate.Typename:
                    newStrat = SortPackByName.Typename;
                    break;
                default:
                    newStrat = SortPackByAddedDate.Typename;
                    break;
            }

            Settings.SelectionSortPackStrategy.Value = newStrat;
            text.text = I18n.S($"Gameplay.Selection.SortPack.{newStrat}");
        }
    }
}
