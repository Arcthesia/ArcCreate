using System;
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

        private readonly HashSet<IStorageUnit> selected = new HashSet<IStorageUnit>();
        private bool isCurrentlyVisible = false;

        public event Action OnClear;

        public bool IsAnySelected => selected.Count > 0;

        public void Add(IStorageUnit item)
        {
            selected.Add(item);
            UpdateState();
        }

        public void Remove(IStorageUnit item)
        {
            selected.Remove(item);
            UpdateState();
        }

        public bool IsStorageSelected(IStorageUnit item)
            => selected.Contains(item);

        public void ClearSelection()
        {
            selected.Clear();
            UpdateState();
            OnClear?.Invoke();
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

            promptText.text = selected.Count >= 2 ?
                              I18n.S("Gameplay.Selection.SelectStatus.Plural", selected.Count) :
                              I18n.S("Gameplay.Selection.SelectStatus.Singular", selected.Count);
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
            deleteConfirmation.PromptUser(selected);
        }
    }
}
