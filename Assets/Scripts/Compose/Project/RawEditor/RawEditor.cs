using System.IO;
using System.Threading;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay;
using ArcCreate.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public class RawEditor : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private FastInputField inputField;
        [SerializeField] private long maxFileLength = 1000000;
        [SerializeField] private GameObject disabledNotify;
        [SerializeField] private FaultNavigation faultNavigation;

        [Header("Components")]
        [SerializeField] private GameObject lineHighlightPrefab;
        [SerializeField] private GameObject scrollHighlightPrefab;
        [SerializeField] private RectTransform lineHighlightParent;
        [SerializeField] private RectTransform scrollHighlightParent;

        private Pool<LineHighlightComponent> lineHighlightPool;
        private Pool<ScrollHighlightComponent> scrollHighlightPool;

        private string absoluteMainChartPath;
        private string rawChartData;

        private readonly ChartAnalyzer analyzer = new ChartAnalyzer();
        private bool analyzeOnNextEnable;
        private bool loadFromPathOnNextEnable;
        private bool reloadOnNextEnable;
        private bool applyChangeOnNextEndEdit;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public void LoadFromPath(string absoluteMainChartPath)
        {
            this.absoluteMainChartPath = absoluteMainChartPath;

            if (!transform.parent.gameObject.activeInHierarchy)
            {
                loadFromPathOnNextEnable = true;
                return;
            }

            long fileSize = new FileInfo(absoluteMainChartPath).Length;
            bool tooLarge = fileSize > maxFileLength;
            if (tooLarge)
            {
                gameObject.SetActive(false);
                disabledNotify.SetActive(true);
                return;
            }

            gameObject.SetActive(true);
            disabledNotify.SetActive(false);
            rawChartData = File.ReadAllText(absoluteMainChartPath);
            UpdateDisplay();
        }

        private void ApplyChanges()
        {
            int timing = Services.Gameplay.Audio.AudioTiming;
            ChartReader reader = ChartReaderFactory.GetReader(new RawEditorFileAccess(rawChartData, absoluteMainChartPath), absoluteMainChartPath);
            reader.Parse();
            gameplayData.LoadChart(reader, "file:///" + Path.GetDirectoryName(absoluteMainChartPath));
            Services.Gameplay.Audio.AudioTiming = timing;
        }

        private void Awake()
        {
            Values.OnEditAction += ReloadEditor;
            inputField.onEndEdit.AddListener(OnEndEdit);
            inputField.onValueChanged.AddListener(OnValueChanged);
            inputField.OnScrollVerticalChanged += OnInputFieldScrollVertical;
            inputField.OnScrollHorizontalChanged += OnInputFieldScrollHorizontal;

            inputField.textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            inputField.textComponent.verticalOverflow = VerticalWrapMode.Truncate;

            lineHighlightPool = Pools.New<LineHighlightComponent>("RawEditor.LineHighlight", lineHighlightPrefab, lineHighlightParent, 10);
            scrollHighlightPool = Pools.New<ScrollHighlightComponent>("RawEditor.ScrollHighlight", scrollHighlightPrefab, scrollHighlightParent, 10);
        }

        private void OnDestroy()
        {
            Values.OnEditAction -= ReloadEditor;
            inputField.onEndEdit.RemoveListener(OnEndEdit);
            inputField.onValueChanged.RemoveListener(OnValueChanged);
            inputField.OnScrollVerticalChanged -= OnInputFieldScrollVertical;
            inputField.OnScrollHorizontalChanged -= OnInputFieldScrollHorizontal;
        }

        private void Update()
        {
            if (analyzer.CheckQueue(out var fault))
            {
                DisplayFault(fault);
            }
        }

        private void OnInputFieldScrollVertical(float y)
        {
            lineHighlightParent.anchoredPosition = new Vector2(lineHighlightParent.anchoredPosition.x, y);
        }

        private void OnInputFieldScrollHorizontal(float x)
        {
            lineHighlightParent.anchoredPosition = new Vector2(x, lineHighlightParent.anchoredPosition.y);
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
                Analyze();
                analyzeOnNextEnable = false;
            }

            lineHighlightParent.SetAsLastSibling();
        }

        private void ReloadEditor()
        {
            if (!gameObject.activeInHierarchy)
            {
                reloadOnNextEnable = true;
                return;
            }

            var chartData = new RawEventsBuilder().GetEvents(Path.GetFileName(absoluteMainChartPath), false);
            rawChartData = new ChartSerializer(null, "").WriteToString(
                absoluteMainChartPath,
                gameplayData.AudioOffset.Value,
                gameplayData.TimingPointDensityFactor.Value,
                chartData);

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            inputField.text = rawChartData;
            Analyze();
        }

        private void Analyze()
        {
            if (!gameObject.activeInHierarchy)
            {
                analyzeOnNextEnable = true;
                return;
            }

            ClearHighlights();
            analyzer.Stop();
            analyzer.Start(rawChartData, absoluteMainChartPath);
        }

        private void OnEndEdit(string val)
        {
            if (applyChangeOnNextEndEdit)
            {
                applyChangeOnNextEndEdit = false;
                cts.Cancel();
                cts.Dispose();
                cts = new CancellationTokenSource();

                OnEndEditTask(val, cts.Token).Forget();
            }
        }

        private async UniTask OnEndEditTask(string val, CancellationToken ct)
        {
            rawChartData = val;
            ClearHighlights();
            analyzer.Stop();
            analyzer.Start(rawChartData, absoluteMainChartPath);
            while (true)
            {
                if (analyzer.HasError || ct.IsCancellationRequested)
                {
                    return;
                }

                if (analyzer.IsComplete)
                {
                    ApplyChanges();
                    return;
                }

                await UniTask.NextFrame();
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
            analyzer.Start(rawChartData, absoluteMainChartPath);
        }

        private void DisplayFault(ChartFault fault)
        {
            LineHighlightComponent line = lineHighlightPool.Get();
            line.SetPosition(inputField.TextGenerator, fault.LineNumber, fault.StartCharPos, fault.Length);
            line.SetContent(fault.Severity, fault.Description);

            ScrollHighlightComponent scroll = scrollHighlightPool.Get();
            scroll.SetPosition(inputField.TextGenerator, fault.LineNumber);
            scroll.SetSeverity(fault.Severity);

            faultNavigation.RegisterFault(fault);
        }

        private void ClearHighlights()
        {
            lineHighlightPool.ReturnAll();
            scrollHighlightPool.ReturnAll();
            faultNavigation.Clear();
        }
    }
}
