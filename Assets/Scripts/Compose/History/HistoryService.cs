using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.History
{
    public class HistoryService : MonoBehaviour, IHistoryService
    {
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        [SerializeField] private TMP_Text undoCountText;
        [SerializeField] private TMP_Text redoCountText;

        private readonly Stack<ICommand> undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> redoStack = new Stack<ICommand>();

        public DateTime LastEdit { get; private set; }

        public void AddCommand(ICommand command)
        {
            undoStack.Push(command);
            redoStack.Clear();
            command.Execute();

            OnCommand();
        }

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
        }

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

            LastEdit = DateTime.Now;
        }
    }
}