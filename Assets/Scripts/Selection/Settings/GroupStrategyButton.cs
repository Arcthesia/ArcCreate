using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    [RequireComponent(typeof(Button))]
    public class GroupStrategyButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(CycleGroupStrategy);
            I18n.OnLocaleChanged += OnLocale;
            OnLocale();
        }

        private void OnDestroy()
        {
            I18n.OnLocaleChanged -= OnLocale;
            GetComponent<Button>().onClick.RemoveListener(CycleGroupStrategy);
        }

        private void OnLocale()
        {
            text.text = I18n.S($"Gameplay.Selection.Group.{Settings.SelectionGroupStrategy.Value}");
        }

        private void CycleGroupStrategy()
        {
            string newStrat = null;
            switch (Settings.SelectionGroupStrategy.Value)
            {
                case NoGroup.Typename:
                    newStrat = GroupByDifficulty.Typename;
                    break;
                case GroupByDifficulty.Typename:
                    newStrat = GroupByGrade.Typename;
                    break;
                case GroupByGrade.Typename:
                    newStrat = GroupByRank.Typename;
                    break;
                case GroupByRank.Typename:
                    newStrat = GroupByCharter.Typename;
                    break;
                case GroupByCharter.Typename:
                    newStrat = NoGroup.Typename;
                    break;
                default:
                    newStrat = NoGroup.Typename;
                    break;
            }

            Settings.SelectionGroupStrategy.Value = newStrat;
            text.text = I18n.S($"Gameplay.Selection.Group.{newStrat}");
        }
    }
}