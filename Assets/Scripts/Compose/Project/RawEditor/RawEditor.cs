using System.IO;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class RawEditor : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Scrollbar horizontalScrollbar;
        [SerializeField] private float minWidth;
        [SerializeField] private float maxWidth;
        private string currentMainChartPath;
        private string rawChartData;
        private bool reloadOnNextEnable;
        private RectTransform inputRect;
        private RectTransform textRect;
        private RectTransform rect;
        private RectTransform caretRect;
        private Vector2 previousSize;

        public void LoadFromPath(string absoluteMainChartPath)
        {
            currentMainChartPath = Path.GetFileName(absoluteMainChartPath);
            rawChartData = File.ReadAllText(absoluteMainChartPath);
            UpdateDisplay();
        }

        private void ApplyChanges()
        {
            int timing = Services.Gameplay.Audio.AudioTiming;
            ChartReader reader = ChartReaderFactory.GetReader(new VirtualFileAccess(rawChartData), currentMainChartPath);
            reader.Parse();
            gameplayData.LoadChart(reader, "file:///" + Path.GetDirectoryName(currentMainChartPath));
            Services.Gameplay.Audio.AudioTiming = timing;
        }

        private void Awake()
        {
            Values.OnEditAction += ReloadEditor;
            inputField.onEndEdit.AddListener(OnEndEdit);
            horizontalScrollbar.onValueChanged.AddListener(OnHorizontalScroll);

            inputRect = inputField.GetComponent<RectTransform>();
            rect = GetComponent<RectTransform>();
            textRect = inputField.textComponent.GetComponent<RectTransform>();

            inputField.textComponent.enableWordWrapping = false;
        }

        private void OnDestroy()
        {
            Values.OnEditAction -= ReloadEditor;
            inputField.onEndEdit.RemoveListener(OnEndEdit);
            horizontalScrollbar.onValueChanged.RemoveListener(OnHorizontalScroll);
        }

        private void Update()
        {
            inputField.textComponent.enabled = inputRect.rect.size == previousSize;
            if (inputRect.rect.size != previousSize)
            {
                float width = inputField.textComponent.preferredWidth;
                horizontalScrollbar.size = Mathf.Clamp(rect.rect.size.x / width, 0, 1);
            }

            previousSize = inputRect.rect.size;
        }

        private void OnEnable()
        {
            if (reloadOnNextEnable)
            {
                ReloadEditor();
                reloadOnNextEnable = false;
            }
        }

        private void ReloadEditor()
        {
            if (!gameObject.activeInHierarchy)
            {
                reloadOnNextEnable = true;
                return;
            }

            var chartData = new RawEventsBuilder().GetEvents(Path.GetFileName(currentMainChartPath), false);
            rawChartData = new ChartSerializer(null, "").WriteToString(
                currentMainChartPath,
                gameplayData.AudioOffset.Value,
                gameplayData.TimingPointDensityFactor.Value,
                chartData);

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            inputField.text = rawChartData;
            float width = inputField.textComponent.preferredWidth;
            horizontalScrollbar.size = Mathf.Clamp(rect.rect.size.x / width, 0, 1);
        }

        private void OnHorizontalScroll(float val)
        {
            float width = inputField.textComponent.preferredWidth;
            float x = val * width;

            if (caretRect == null)
            {
                caretRect = GetComponentInChildren<TMP_SelectionCaret>().GetComponent<RectTransform>();
            }

            textRect.anchoredPosition = new Vector2(-x, textRect.anchoredPosition.y);
            caretRect.anchoredPosition = new Vector2(-x, textRect.anchoredPosition.y);
        }

        private void OnEndEdit(string val)
        {
            if (val != rawChartData)
            {
                rawChartData = val;
                ApplyChanges();
            }
        }
    }
}
