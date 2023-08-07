using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Animation;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Select
{
    public class SubAssetDeleteConfirmation : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text listText;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button keepButton;
        [SerializeField] private ScriptedAnimator animator;

        private bool deletePressed;
        private bool endPrompt;

        // Only handles pack for now. As if we're gonna add more asset types right?
        public async UniTask<IEnumerable<IStorageUnit>> Prompt(PackStorage unit)
        {
            endPrompt = false;
            StringBuilder str = new StringBuilder();
            List<IStorageUnit> storageUnits = unit.Levels.ToList<IStorageUnit>();
            foreach (var st in storageUnits)
            {
                if (!st.IsDefaultAsset)
                {
                    str.AppendLine($"{st.Type}: {st.Identifier}");
                }
            }

            listText.text = str.ToString();
            titleText.text = I18n.S("Gameplay.Selection.PackDeleteConfirmation", unit.Identifier);
            animator.Show();

            await UniTask.WaitUntil(() => endPrompt);
            if (deletePressed)
            {
                return storageUnits;
            }
            else
            {
                return Enumerable.Empty<IStorageUnit>();
            }
        }

        private void Awake()
        {
            deleteButton.onClick.AddListener(Delete);
            keepButton.onClick.AddListener(Cancel);
        }

        private void OnDestroy()
        {
            deleteButton.onClick.RemoveListener(Delete);
            keepButton.onClick.RemoveListener(Cancel);
        }

        private void Cancel()
        {
            deletePressed = false;
            endPrompt = true;
            animator.Hide();
        }

        private void Delete()
        {
            deletePressed = true;
            endPrompt = true;
            animator.Hide();
        }
    }
}