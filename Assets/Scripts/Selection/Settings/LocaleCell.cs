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

        public override UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            return default;
        }

        public override void SetCellData(CellData cellData)
        {
            text.text = (cellData as LocaleCellData).Name;
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
            Settings.Locale.Value = text.text;
        }
    }
}