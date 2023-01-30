﻿using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Navigation
{
    [EditorScope("ContextMenu")]
    public class ContextMenu : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private GameObject contextButtonPrefab;
        [SerializeField] private GameObject contextCategoryPrefab;
        [SerializeField] private float minWidth = 100;
        [SerializeField] private float padding = 5;
        [SerializeField] private float minDistanceFromBorder = 50;
        private RectTransform rect;

        private Pool<ContextMenuCategory> categoryPool;
        private Pool<ContextMenuButton> buttonPool;

        private readonly Dictionary<string, ContextMenuCategory> categories = new Dictionary<string, ContextMenuCategory>();

        [EditorAction("Open", false, "<mouse2>")]
        [SubAction("Close", false, "<u-mouse1>", "<mouse2><u-mouse2>", "<u-mouse3>", "<esc>")]
        public async UniTask OpenContextMenu(EditorAction action)
        {
            SubAction close = action.GetSubAction("Close");

            buttonPool.ReturnAll();
            categoryPool.ReturnAll();
            categories.Clear();

            bool optionsExist = false;
            float height = 0;
            foreach (IAction entry in Services.Navigation.GetContextMenuEntries(true))
            {
                if (categories.TryGetValue(entry.Category, out ContextMenuCategory category))
                {
                    ContextMenuButton button = buttonPool.Get(category.transform);
                    category.ConfigureButton(button);
                    button.Setup(entry, this);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(button.Width + (padding * 2), rect.rect.width));
                    height += button.Rect.rect.height;
                }
                else
                {
                    ContextMenuCategory newCategory = categoryPool.Get();
                    newCategory.transform.SetAsLastSibling();
                    newCategory.ResetSize();
                    newCategory.SetText(entry.Category);
                    height += newCategory.BaseHeight;

                    categories.Add(entry.Category, newCategory);
                    ContextMenuButton button = buttonPool.Get(newCategory.transform);
                    newCategory.ConfigureButton(button);
                    button.Setup(entry, this);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(button.Width + (padding * 2), rect.rect.width));
                    height += button.Rect.rect.height;
                }

                optionsExist = true;
            }

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(minWidth, rect.rect.width));
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            AlignToCursor(height);

            if (optionsExist)
            {
                parent.gameObject.SetActive(true);
                await UniTask.WaitUntil(() => close.WasExecuted);

                // No button pressed
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    CloseContextMenu();
                }
            }
        }

        public void CloseContextMenu()
        {
            parent.gameObject.SetActive(false);
        }

        private void AlignToCursor(float rectHeight)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePosition, null, out Vector2 position);

            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;
            float rectWidth = rect.rect.width;
            Vector2 pivot = rect.pivot;

            float x = Mathf.Clamp(
                position.x,
                -(canvasWidth / 2) + minDistanceFromBorder + (rectWidth * pivot.x),
                (canvasWidth / 2) - minDistanceFromBorder - (rectWidth * (1 - pivot.x)));

            float y = Mathf.Clamp(
                position.y,
                -(canvasHeight / 2) + minDistanceFromBorder + (rectHeight * pivot.y),
                (canvasHeight / 2) - minDistanceFromBorder - (rectHeight * (1 - pivot.y)));

            rect.anchoredPosition = new Vector2(x, y);
        }

        private void Awake()
        {
            buttonPool = Pools.New<ContextMenuButton>(contextButtonPrefab.name, contextButtonPrefab, parent, 10);
            categoryPool = Pools.New<ContextMenuCategory>(contextCategoryPrefab.name, contextCategoryPrefab, parent, 5);
            rect = GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
        }
    }
}