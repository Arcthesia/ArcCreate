using System.Threading;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class LocaleCell : Cell
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text text;
        private string id;

        public override UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            return default;
        }

        public override void SetCellData(CellData cellData)
        {
            text.text = (cellData as LocaleCellData).Name;
            id = (cellData as LocaleCellData).Id;
        }

        private void Awake()
        {
            button.onClick.AddListener(OnButton);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButton);
        }

        private void OnButton()
        {
            Settings.Locale.Value = id;
            I18n.SetLocale(Settings.Locale.Value);
        }
    }
}