using System.Threading;
using ArcCreate.Selection.Select;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class PackCell : Cell
    {
        [SerializeField] private StorageData storage;
        [SerializeField] private SelectableStorage selectable;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text title;
        [SerializeField] private RawImage image;

        private PackStorage pack;

        public override void SetCellData(CellData cellData)
        {
            PackCellData data = cellData as PackCellData;
            pack = data.PackStorage;
            selectable.StorageUnit = pack;
            title.text = pack.PackName;

            if (storage.TryAssignTextureFromCache(image, pack, pack.ImagePath))
            {
                MarkFullyLoaded();
            }
        }

        public override async UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            await storage.AssignTexture(image, pack, pack.ImagePath);
        }

        private void Awake()
        {
            button.onClick.AddListener(SelectSelf);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(SelectSelf);
        }

        private void SelectSelf()
        {
            if (Services.Select.IsAnySelected || storage.IsTransitioning)
            {
                return;
            }

            storage.SelectedPack.Value = pack;
        }
    }
}