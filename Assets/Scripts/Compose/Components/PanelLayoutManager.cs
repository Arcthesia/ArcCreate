using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPanels;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class PanelLayoutManager : MonoBehaviour, IItemNameDialogConsumer
    {
        private const string PlayerPrefKey = "Compose.CustomPanelLayout";

        [SerializeField] private Button togglePickerButton;
        [SerializeField] private GameObject picker;
        [SerializeField] private DynamicPanelsCanvas canvas;
        [SerializeField] private Button defaultLayoutButton;
        [SerializeField] private Button saveLayoutButton;
        [SerializeField] private SaveItemNameDialog saveLayoutDialog;
        [SerializeField] private GameObject customLayoutRowPrefab;
        [SerializeField] private Transform customLayoutRowsParent;
        private readonly List<PanelLayoutRow> rows = new List<PanelLayoutRow>();
        private Dictionary<string, byte[]> customLayouts = new Dictionary<string, byte[]>();
        private byte[] defaultLayoutData;

        bool IItemNameDialogConsumer.IsValidName(string text, out string reason)
        {
            if (customLayouts.ContainsKey(text))
            {
                reason = I18n.S("Compose.Dialog.PanelLayout.DuplicateError");
            }

            reason = string.Empty;
            return true;
        }

        void IItemNameDialogConsumer.SaveItem(string label)
        {
            byte[] data = PanelSerialization.SerializeCanvasToArray(canvas);
            customLayouts.Add(label, data);

            GameObject go = Instantiate(customLayoutRowPrefab, customLayoutRowsParent);
            PanelLayoutRow newRow = go.GetComponent<PanelLayoutRow>();
            rows.Add(newRow);
            newRow.SetData(this, data, label);
        }

        internal void Remove(string label)
        {
            customLayouts.Remove(label);
            foreach (var row in rows)
            {
                if (row.Label == label)
                {
                    Destroy(row.gameObject);
                }
            }

            rows.RemoveAll(r => r.Label == label);
        }

        internal void Select(byte[] data)
        {
            byte[] fallback = PanelSerialization.SerializeCanvasToArray(canvas);

            try
            {
                PanelSerialization.DeserializeCanvasFromArray(canvas, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                PanelSerialization.DeserializeCanvasFromArray(canvas, fallback);
            }
        }

        private void Awake()
        {
            defaultLayoutButton.onClick.AddListener(SetDefaultLayout);
            saveLayoutButton.onClick.AddListener(OnSaveLayoutButton);
            togglePickerButton.onClick.AddListener(TogglePicker);
            Application.quitting += SaveLayoutForNextSession;
            customLayouts = LoadCustomLayouts();
            foreach (var pair in customLayouts)
            {
                GameObject go = Instantiate(customLayoutRowPrefab, customLayoutRowsParent);
                PanelLayoutRow newRow = go.GetComponent<PanelLayoutRow>();
                rows.Add(newRow);
                newRow.SetData(this, pair.Value, pair.Key);
            }
        }

        private void OnDestroy()
        {
            defaultLayoutButton.onClick.RemoveListener(SetDefaultLayout);
            saveLayoutButton.onClick.RemoveListener(OnSaveLayoutButton);
            togglePickerButton.onClick.RemoveListener(TogglePicker);
            Application.quitting -= SaveLayoutForNextSession;
            SaveLayoutForNextSession();
            SaveCustomLayouts(customLayouts);
        }

        private void TogglePicker()
        {
            picker.SetActive(!picker.activeSelf);
        }

        private void Start()
        {
            defaultLayoutData = PanelSerialization.SerializeCanvasToArray(canvas);
            LoadLayoutFromLastSession();
        }

        private void OnSaveLayoutButton()
        {
            saveLayoutDialog.Open(this);
        }

        private void SetDefaultLayout()
        {
            PanelSerialization.DeserializeCanvasFromArray(canvas, defaultLayoutData);
        }

        private void SaveLayoutForNextSession()
        {
            PanelSerialization.SerializeCanvas(canvas);
        }

        private void LoadLayoutFromLastSession()
        {
            PanelSerialization.DeserializeCanvas(canvas);
        }

        private Dictionary<string, byte[]> LoadCustomLayouts()
        {
            string data = PlayerPrefs.GetString(PlayerPrefKey, null);
            if (data == null)
            {
                return null;
            }

            try
            {
                var deserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                return deserialized.ToDictionary(
                    pair => pair.Key,
                    pair => Convert.FromBase64String(pair.Value));
            }
            catch
            {
                return new Dictionary<string, byte[]>();
            }
        }

        private void SaveCustomLayouts(Dictionary<string, byte[]> customLayouts)
        {
            var stringConverted = customLayouts.ToDictionary(
                pair => pair.Key,
                pair => Convert.ToBase64String(pair.Value));

            var serialized = JsonConvert.SerializeObject(stringConverted);
            PlayerPrefs.SetString(PlayerPrefKey, serialized);
        }
    }
}