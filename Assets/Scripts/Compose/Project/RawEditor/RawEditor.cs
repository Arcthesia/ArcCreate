using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay;
using Cysharp.Threading.Tasks;
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

        [Header("Components")]
        [SerializeField] private GameObject lineHighlightPrefab;
        [SerializeField] private GameObject scrollHighlightPrefab;
        [SerializeField] private RectTransform lineHighlightParent;
        [SerializeField] private RectTransform scrollHighlightParent;

        private Pool<LineHighlightComponent> lineHighlightPool;
        private Pool<ScrollHighlightComponent> scrollHighlightPool;

        private string absoluteMainChartPath;
        private string currentMainChartPath;
        private string rawChartData;
        private RectTransform inputRect;
        private RectTransform textRect;
        private RectTransform rect;
        private RectTransform caretRect;
        private Vector2 previousSize;

        private readonly ChartAnalyzer analyzer = new ChartAnalyzer();
        private bool analyzeOnNextEnable;
        private bool loadFromPathOnNextEnable;
        private bool reloadOnNextEnable;
        private bool applyChangeOnNextEndEdit;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public void LoadFromPath(string absoluteMainChartPath)
        {
            this.absoluteMainChartPath = absoluteMainChartPath;
            currentMainChartPath = Path.GetFileName(absoluteMainChartPath);

            if (!gameObject.activeInHierarchy)
            {
                loadFromPathOnNextEnable = true;
                return;
            }

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
            inputField.onValueChanged.AddListener(OnValueChanged);
            horizontalScrollbar.onValueChanged.AddListener(OnHorizontalScroll);

            inputRect = inputField.GetComponent<RectTransform>();
            rect = GetComponent<RectTransform>();
            textRect = inputField.textComponent.GetComponent<RectTransform>();

            inputField.textComponent.enableWordWrapping = false;
            lineHighlightPool = Pools.New<LineHighlightComponent>("RawEditor.LineHighlight", lineHighlightPrefab, lineHighlightParent, 10);
            scrollHighlightPool = Pools.New<ScrollHighlightComponent>("RawEditor.ScrollHighlight", scrollHighlightPrefab, scrollHighlightParent, 10);
        }

        private void OnDestroy()
        {
            Values.OnEditAction -= ReloadEditor;
            inputField.onEndEdit.RemoveListener(OnEndEdit);
            inputField.onValueChanged.RemoveListener(OnValueChanged);
            horizontalScrollbar.onValueChanged.RemoveListener(OnHorizontalScroll);
        }

        private void Update()
        {
            inputField.textComponent.enabled = inputRect.rect.size == previousSize;
            lineHighlightParent.anchoredPosition = textRect.anchoredPosition;
            if (inputRect.rect.size != previousSize)
            {
                float width = inputField.textComponent.preferredWidth;
                horizontalScrollbar.size = Mathf.Clamp(rect.rect.size.x / width, 0, 1);
            }

            previousSize = inputRect.rect.size;
            if (analyzer.CheckQueue(out var fault))
            {
                DisplayFault(fault);
            }
        }

        private void OnEnable()
        {
            if (reloadOnNextEnable)
            {
                ReloadEditor();
                reloadOnNextEnable = false;
                analyzeOnNextEnable = false;
                loadFromPathOnNextEnable = false;
            }

            if (loadFromPathOnNextEnable)
            {
                LoadFromPath(absoluteMainChartPath);
                loadFromPathOnNextEnable = false;
                analyzeOnNextEnable = false;
            }

            if (analyzeOnNextEnable)
            {
                cts.Cancel();
                cts.Dispose();
                cts = new CancellationTokenSource();
                Analyze(cts.Token).Forget();
                analyzeOnNextEnable = false;
            }

            lineHighlightParent.SetAsFirstSibling();
            var caret = GetComponentInChildren<TMP_SelectionCaret>();
            if (caret != null)
            {
                caret.raycastTarget = false;
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

            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            Analyze(cts.Token).Forget();
        }

        private async UniTask Analyze(CancellationToken ct)
        {
            if (!gameObject.activeInHierarchy)
            {
                analyzeOnNextEnable = true;
                return;
            }

            ClearHighlights();
            analyzer.Stop();
            analyzer.Start(rawChartData, currentMainChartPath);
            await UniTask.WaitUntil(() => analyzer.IsComplete, cancellationToken: ct).SuppressCancellationThrow();
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
            if (applyChangeOnNextEndEdit)
            {
                applyChangeOnNextEndEdit = false;
                rawChartData = val;
                cts.Cancel();
                cts.Dispose();
                cts = new CancellationTokenSource();

                ClearHighlights();
                analyzer.Stop();
                analyzer.Start(rawChartData, currentMainChartPath);
                while (true)
                {
                    if (analyzer.HasError)
                    {
                        return;
                    }

                    if (analyzer.IsComplete)
                    {
                        ApplyChanges();
                        return;
                    }
                }
            }
        }

        private void OnValueChanged(string val)
        {
            if (gameObject.activeInHierarchy && val != rawChartData)
            {
                applyChangeOnNextEndEdit = true;
                cts.Cancel();
                cts.Dispose();
                cts = new CancellationTokenSource();
                OnValueChangedTask(val, cts.Token).Forget();
            }
        }

        private async UniTask OnValueChangedTask(string val, CancellationToken ct)
        {
            bool cancelled = await UniTask.Delay(1000, cancellationToken: ct).SuppressCancellationThrow();
            if (cancelled)
            {
                return;
            }

            rawChartData = val;
            ClearHighlights();
            analyzer.Stop();
            analyzer.Start(rawChartData, currentMainChartPath);
            while (true)
            {
                await UniTask.NextFrame();

                if (ct.IsCancellationRequested || analyzer.IsComplete)
                {
                    return;
                }
            }
        }

        private void DisplayFault(ChartFault fault)
        {
            LineHighlightComponent line = lineHighlightPool.Get();
            line.SetPosition(inputField, fault.LineNumber, fault.StartCharPos, fault.EndCharPos);
            line.SetContent(fault.Severity, fault.Description);

            ScrollHighlightComponent scroll = scrollHighlightPool.Get();
            scroll.SetPosition(inputField, fault.LineNumber);
            scroll.SetSeverity(fault.Severity);
        }

        private void ClearHighlights()
        {
            lineHighlightPool.ReturnAll();
            scrollHighlightPool.ReturnAll();
        }
    }
}
