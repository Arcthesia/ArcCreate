using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Animation;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Select
{
    public class DeleteConfirmation : MonoBehaviour
    {
        [SerializeField] private StorageData storageData;
        [SerializeField] private SubAssetDeleteConfirmation subAssetDeleteConfirmation;
        [SerializeField] private TMP_Text listText;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ScriptedAnimator animator;

        private List<IStorageUnit> storageUnits;

        public void PromptUser(IEnumerable<IStorageUnit> units)
        {
            StartPrompt(units).Forget();
        }

        private async UniTask StartPrompt(IEnumerable<IStorageUnit> units)
        {
            storageUnits = units.ToList();
            foreach (var unit in units)
            {
                if (unit is PackStorage pack)
                {
                    IEnumerable<IStorageUnit> subAssets = await subAssetDeleteConfirmation.Prompt(pack);
                    storageUnits.AddRange(subAssets);
                }
            }

            StringBuilder str = new StringBuilder();
            foreach (var st in storageUnits)
            {
                if (!st.IsDefaultAsset)
                {
                    str.AppendLine($"{st.Type}: {st.Identifier}");
                }
            }

            listText.text = str.ToString();
            animator.Show();
        }

        private void Awake()
        {
            deleteButton.onClick.AddListener(Delete);
            cancelButton.onClick.AddListener(Cancel);
        }

        private void OnDestroy()
        {
            deleteButton.onClick.RemoveListener(Delete);
            cancelButton.onClick.RemoveListener(Cancel);
        }

        private void Cancel()
        {
            animator.Hide();
        }

        private void Delete()
        {
            foreach (var st in storageUnits)
            {
                if (!st.IsDefaultAsset)
                {
                    st.Delete();
                }
            }

            storageUnits.Clear();
            animator.Hide();
            Services.Select.ClearSelection();
            storageData.NotifyStorageChange();
        }
    }
}