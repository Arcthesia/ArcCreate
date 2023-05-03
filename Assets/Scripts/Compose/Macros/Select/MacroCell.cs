using System.Threading;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using Google.MaterialDesign.Icons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Macros
{
    public class MacroCell : Cell
    {
        [SerializeField] private MaterialIcon icon;
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform rect;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;
        [SerializeField] private string folderIcon;
        [SerializeField] private float indentWidth;
        [SerializeField] private float offsetMinWithIcon;
        [SerializeField] private float offsetMinWithoutIcon;
        private string macroId;
        private bool isFolder;

        public override UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            return default;
        }

        public override void SetCellData(CellData cellData)
        {
            MacroDefinition macro = cellData as MacroDefinition;
            macroId = macro.Id;
            text.text = macro.Name;

            isFolder = macro.Callback == null;
            bool hasIcon = macro.Icon != null || isFolder;
            icon.iconUnicode = macro.Icon ?? folderIcon;
            icon.gameObject.SetActive(hasIcon);
            rect.offsetMin = new Vector2(hasIcon ? offsetMinWithIcon : offsetMinWithoutIcon, rect.offsetMin.y);

            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(indentWidth * HierarchyData.IndentDepth, 0);
            content.offsetMax = Vector2.zero;
        }

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (!isFolder)
            {
                Services.Macros.RunMacro(macroId);
            }
            else
            {
                ToggleCollapse();
                MacroDefinition.ToggleCollapseByDefault(macroId);
            }
        }
    }
}