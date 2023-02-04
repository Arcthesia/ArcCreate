using System;
using System.Collections.Generic;
using ArcCreate.Compose.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.History
{
    [EditorScope("History")]
    public class HistoryService : MonoBehaviour, IHistoryService
    {
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        [SerializeField] private TMP_Text undoCountText;
        [SerializeField] private TMP_Text redoCountText;

        private readonly Stack<ICommand> undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> redoStack = new Stack<ICommand>();

        public int UndoCount => undoStack.Count;

        public int RedoCount => redoStack.Count;

        public void AddCommand(ICommand command)
        {
            undoStack.Push(command);
            redoStack.Clear();
            command.Execute();

            OnCommand();
            Notify("Compose.Notify.History.Execute", command);
        }

        [EditorAction(null, false, "<c-y>", "<c-Z>")]
        [RequireRedoStack]
        public void Redo()
        {
            if (redoStack.Count <= 0)
            {
                return;
            }

            ICommand cmd = redoStack.Pop();
            cmd.Execute();
            undoStack.Push(cmd);

            OnCommand();
            Notify("Compose.Notify.History.Redo", cmd);
        }

        [EditorAction(null, false, "<c-z>")]
        [RequireUndoStack]
        public void Undo()
        {
            if (undoStack.Count <= 0)
            {
                return;
            }

            ICommand cmd = undoStack.Pop();
            cmd.Undo();
            redoStack.Push(cmd);

            OnCommand();
            Notify("Compose.Notify.History.Undo", cmd);
        }

        private void Awake()
        {
            undoButton.onClick.AddListener(Undo);
            redoButton.onClick.AddListener(Redo);
        }

        private void OnDestroy()
        {
            undoButton.onClick.RemoveListener(Undo);
            redoButton.onClick.RemoveListener(Redo);
        }

        private void OnCommand()
        {
            undoCountText.text = undoStack.Count.ToString();
            redoCountText.text = redoStack.Count.ToString();

            undoButton.interactable = undoStack.Count > 0;
            redoButton.interactable = redoStack.Count > 0;

            Values.ProjectModified = true;
        }

        private void Notify(string i18nKey, ICommand cmd)
        {
            Debug.Log(I18n.S(i18nKey, new Dictionary<string, object>
            {
                { "Name", cmd.Name },
            }));
        }

        public class RequireUndoStackAttribute : ContextRequirementAttribute
        {
            public override bool CheckRequirement()
            {
                return Services.History.UndoCount > 0;
            }
        }

        public class RequireRedoStackAttribute : ContextRequirementAttribute
        {
            public override bool CheckRequirement()
            {
                return Services.History.RedoCount > 0;
            }
        }
    }
}