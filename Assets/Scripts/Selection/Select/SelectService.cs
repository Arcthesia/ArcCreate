using System.Collections.Generic;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Select
{
    public class SelectService : MonoBehaviour, ISelectService
    {
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ScriptedAnimator selectPromptAnimator;
        [SerializeField] private DeleteConfirmation deleteConfirmation;

        private readonly HashSet<Components.Selectable> selected = new HashSet<Components.Selectable>();
        private bool isCurrentlyVisible = false;

        public bool IsAnySelected => selected.Count > 0;

        public void AddComponent(Components.Selectable deletable)
        {
            selected.Add(deletable);
            UpdateState();
        }

        public void RemoveComponent(Components.Selectable deletable)
        {
            selected.Remove(deletable);
            UpdateState();
        }

        public void ClearSelection()
        {
            foreach (var comp in selected)
            {
                comp.DeselectSelf();
            }

            selected.Clear();
        }

        private void UpdateState()
        {
            bool willBeVisible = selected.Count > 0;
            if (!isCurrentlyVisible && willBeVisible)
            {
                selectPromptAnimator.Show();
            }
            else if (isCurrentlyVisible && !willBeVisible)
            {
                selectPromptAnimator.Hide();
            }

            promptText.text = I18n.S("Gameplay.Selection.SelectStatus", selected.Count);
            isCurrentlyVisible = selected.Count > 0;
        }

        private void Awake()
        {
            deleteButton.onClick.AddListener(PromptDeleteSelection);
            cancelButton.onClick.AddListener(ClearSelection);
        }

        private void OnDestroy()
        {
            deleteButton.onClick.RemoveListener(PromptDeleteSelection);
            cancelButton.onClick.RemoveListener(ClearSelection);
        }

        private void PromptDeleteSelection()
        {
            List<IStorageUnit> units = new List<IStorageUnit>();
            foreach (var comp in selected)
            {
                if (comp.StorageUnit != null)
                {
                    units.Add(comp.StorageUnit);
                }
            }

            deleteConfirmation.PromptUser(units);
        }
    }
}
