using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("NotePallete")]
    public class NotePallete : MonoBehaviour
    {
        [SerializeField] private NoteCreation noteCreation;
        [SerializeField] private Button tapButton;
        [SerializeField] private Button holdButton;
        [SerializeField] private Button arcButton;
        [SerializeField] private Button traceButton;
        [SerializeField] private Button arctapButton;
        [SerializeField] private ArcTypeSelector arcTypeSelector;
        [SerializeField] private ArcColorSelector arcColorSelector;
        [SerializeField] private Image arcImage;

        [SerializeField] private GameObject tapButtonHighlight;
        [SerializeField] private GameObject holdButtonHighlight;
        [SerializeField] private GameObject arcButtonHighlight;
        [SerializeField] private GameObject traceButtonHighlight;
        [SerializeField] private GameObject arctapButtonHighlight;

        private bool creatingArc = true;

        [EditorAction("Arc", false, "a")]
        [RequireGameplayLoaded]
        [SubAction("ConfirmColor", false, "<u-a>")]
        [SubAction("Blue", false, "1")]
        [SubAction("Red", false, "2")]
        [SubAction("Green", false, "3")]
        public UniTask SwitchToArc(EditorAction action)
        {
            return ArcAction(action, creatingArc);
        }

        [EditorAction("ArcAlt", false, "<a-a>")]
        [RequireGameplayLoaded]
        [SubAction("ConfirmColor", false, "<u-a>")]
        [SubAction("Blue", false, "1")]
        [SubAction("Red", false, "2")]
        [SubAction("Green", false, "3")]
        public UniTask SwitchToArcAlt(EditorAction action)
        {
            return ArcAction(action, !creatingArc);
        }

        public async UniTask ArcAction(EditorAction action, bool creatingArc)
        {
            SubAction blue = action.GetSubAction("Blue");
            SubAction red = action.GetSubAction("Red");
            SubAction green = action.GetSubAction("Green");
            SubAction confirmColor = action.GetSubAction("ConfirmColor");

            if (creatingArc)
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Arc;
            }
            else
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Trace;
            }

            while (!confirmColor.WasExecuted)
            {
                if (blue.WasExecuted)
                {
                    Values.CreateArcColorMode.Value = 0;
                }

                if (red.WasExecuted)
                {
                    Values.CreateArcColorMode.Value = 1;
                }

                if (green.WasExecuted)
                {
                    Values.CreateArcColorMode.Value = 2;
                }

                await UniTask.NextFrame();
            }
        }

        [EditorAction("Sky", false, "s")]
        [RequireGameplayLoaded]
        public void SwitchToSky()
        {
            Values.CreateNoteMode.Value = CreateNoteMode.ArcTap;
        }

        [EditorAction("Tap", false, "z")]
        [RequireGameplayLoaded]
        public void SwitchToTap()
        {
            Values.CreateNoteMode.Value = CreateNoteMode.Tap;
        }

        [EditorAction("Hold", false, "x")]
        [RequireGameplayLoaded]
        public void SwitchToHold()
        {
            Values.CreateNoteMode.Value = CreateNoteMode.Hold;
        }

        [EditorAction("Idle", false, "w")]
        [RequireGameplayLoaded]
        public void SwitchToIdle()
        {
            Values.CreateNoteMode.Value = CreateNoteMode.Idle;
        }

        [EditorAction("SwitchArcMode", false, "<f1>")]
        [RequireGameplayLoaded]
        public void SwitchArcMode()
        {
            var selectedArcs = GetSelectedArcs();
            if (selectedArcs.Count > 0)
            {
                ModifySelectedArcs(a => a.IsTrace = !a.IsTrace);
                return;
            }

            creatingArc = !creatingArc;
            if (Values.CreateNoteMode.Value == CreateNoteMode.Arc)
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Trace;
            }
            else if (Values.CreateNoteMode.Value == CreateNoteMode.Trace)
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Arc;
            }
        }

        [EditorAction("SwitchArcColor", false, "<f2>")]
        [RequireGameplayLoaded]
        public void SwitchArcColor()
        {
            var selectedArcs = GetSelectedArcs();
            if (selectedArcs.Count > 0)
            {
                ModifySelectedArcs(a => a.Color = 1 - a.Color, a => a.Color == 0 || a.Color == 1);
                return;
            }

            if (Values.CreateArcColorMode.Value == 0 || Values.CreateArcColorMode.Value == 1)
            {
                Values.CreateArcColorMode.Value = 1 - Values.CreateArcColorMode.Value;
            }
        }

        [EditorAction("SwitchArcType", false, "<f3>")]
        [RequireGameplayLoaded]
        public void SwitchArcType()
        {
            var selectedArcs = GetSelectedArcs();
            if (selectedArcs.Count > 0)
            {
                ModifySelectedArcs(a => a.LineType = SwitchLineType(a.LineType));
                return;
            }

            Values.CreateArcTypeMode.Value = SwitchLineType(Values.CreateArcTypeMode.Value);
        }

        [EditorAction("CycleArcType", false, "<f4>")]
        [RequireGameplayLoaded]
        public void CycleArcType()
        {
            var selectedArcs = GetSelectedArcs();
            if (selectedArcs.Count > 0)
            {
                ModifySelectedArcs(a => a.LineType = CycleLineType(a.LineType));
                return;
            }

            Values.CreateArcTypeMode.Value = CycleLineType(Values.CreateArcTypeMode.Value);
        }

        [EditorAction("CycleArcTypeReversed", false, "<f5>")]
        [RequireGameplayLoaded]
        public void CycleArcTypeReversed()
        {
            var selectedArcs = GetSelectedArcs();
            if (selectedArcs.Count > 0)
            {
                ModifySelectedArcs(a => a.LineType = CycleLineTypeReversed(a.LineType));
                return;
            }

            Values.CreateArcTypeMode.Value = CycleLineTypeReversed(Values.CreateArcTypeMode.Value);
        }

        [EditorAction("B", false, "1")]
        [RequireGameplayLoaded]
        public void SetTypeB()
            => SetType(ArcLineType.B);

        [EditorAction("S", false, "2")]
        [RequireGameplayLoaded]
        public void SetTypeS()
            => SetType(ArcLineType.S);

        [EditorAction("Si", false, "3")]
        [RequireGameplayLoaded]
        public void SetTypeSi()
            => SetType(ArcLineType.Si);

        [EditorAction("So", false, "4")]
        [RequireGameplayLoaded]
        public void SetTypeSo()
            => SetType(ArcLineType.So);

        [EditorAction("SiSi", false, "5")]
        [RequireGameplayLoaded]
        public void SetTypeSiSi()
            => SetType(ArcLineType.SiSi);

        [EditorAction("SoSo", false, "6")]
        [RequireGameplayLoaded]
        public void SetTypeSoSo()
            => SetType(ArcLineType.SoSo);

        [EditorAction("SiSo", false, "7")]
        [RequireGameplayLoaded]
        public void SetTypeSiSo()
            => SetType(ArcLineType.SiSo);

        [EditorAction("SoSi", false, "8")]
        [RequireGameplayLoaded]
        public void SetTypeSoSi()
            => SetType(ArcLineType.SoSi);

        private void SetType(ArcLineType type)
        {
            var selectedArcs = GetSelectedArcs();
            if (selectedArcs.Count > 0)
            {
                ModifySelectedArcs(a => a.LineType = type, a => a.LineType != type);
            }

            Values.CreateArcTypeMode.Value = type;
        }

        private void ModifySelectedArcs(Action<Arc> modifier, Func<Arc, bool> requirement = null)
        {
            List<(ArcEvent instance, ArcEvent newValue)> batch = new List<(ArcEvent instance, ArcEvent newValue)>();
            foreach (var note in Services.Selection.SelectedNotes)
            {
                if (note is Arc arc && (requirement?.Invoke(arc) ?? true))
                {
                    ArcEvent newValue = arc.Clone();
                    modifier.Invoke(newValue as Arc);
                    batch.Add((arc, newValue));
                }
            }

            if (batch.Count > 0)
            {
                Services.History.AddCommand(new EventCommand(
                    name: I18n.S("Compose.Notify.History.EditArc"),
                    update: batch));
            }
        }

        private ArcLineType SwitchLineType(ArcLineType type)
        {
            switch (type)
            {
                case ArcLineType.S: return ArcLineType.B;
                case ArcLineType.B: return ArcLineType.S;
                case ArcLineType.Si: return ArcLineType.So;
                case ArcLineType.SiSi: return ArcLineType.SoSo;
                case ArcLineType.SiSo: return ArcLineType.SoSi;
                case ArcLineType.So: return ArcLineType.Si;
                case ArcLineType.SoSo: return ArcLineType.SiSi;
                case ArcLineType.SoSi: return ArcLineType.SiSo;
                default: return ArcLineType.S;
            }
        }

        private ArcLineType CycleLineType(ArcLineType type)
        {
            switch (type)
            {
                case ArcLineType.B: return ArcLineType.S;
                case ArcLineType.S: return ArcLineType.Si;
                case ArcLineType.Si: return ArcLineType.So;
                case ArcLineType.So: return ArcLineType.SiSi;
                case ArcLineType.SiSi: return ArcLineType.SoSo;
                case ArcLineType.SoSo: return ArcLineType.SiSo;
                case ArcLineType.SiSo: return ArcLineType.SoSi;
                case ArcLineType.SoSi: return ArcLineType.B;
                default: return ArcLineType.S;
            }
        }

        private ArcLineType CycleLineTypeReversed(ArcLineType type)
        {
            switch (type)
            {
                case ArcLineType.B: return ArcLineType.SoSi;
                case ArcLineType.S: return ArcLineType.B;
                case ArcLineType.Si: return ArcLineType.S;
                case ArcLineType.So: return ArcLineType.Si;
                case ArcLineType.SiSi: return ArcLineType.So;
                case ArcLineType.SoSo: return ArcLineType.SiSi;
                case ArcLineType.SiSo: return ArcLineType.SoSo;
                case ArcLineType.SoSi: return ArcLineType.SiSo;
                default: return ArcLineType.S;
            }
        }

        private List<Arc> GetSelectedArcs()
        {
            return Services.Selection.SelectedNotes.Where(n => n is Arc).Cast<Arc>().ToList();
        }

        private void Awake()
        {
            tapButton.onClick.AddListener(OnTapButton);
            holdButton.onClick.AddListener(OnHoldButton);
            arcButton.onClick.AddListener(OnArcButton);
            traceButton.onClick.AddListener(OnTraceButton);
            arctapButton.onClick.AddListener(OnArcTapButton);
            arcTypeSelector.OnTypeChanged += OnArcTypeSelector;
            arcColorSelector.OnColorChanged += OnArcColorSelector;
            Values.CreateNoteMode.OnValueChange += UpdateModePalleteVisual;
            Values.CreateArcTypeMode.OnValueChange += UpdateArcTypeVisual;
            Values.CreateArcColorMode.OnValueChange += UpdateArcColorVisual;

            UpdateModePalleteVisual(Values.CreateNoteMode.Value);
            UpdateArcTypeVisual(Values.CreateArcTypeMode.Value);
        }

        private void OnDestroy()
        {
            tapButton.onClick.RemoveListener(OnTapButton);
            holdButton.onClick.RemoveListener(OnHoldButton);
            arcButton.onClick.RemoveListener(OnArcButton);
            traceButton.onClick.RemoveListener(OnTraceButton);
            arctapButton.onClick.RemoveListener(OnArcTapButton);
            arcTypeSelector.OnTypeChanged -= OnArcTypeSelector;
            arcColorSelector.OnColorChanged -= OnArcColorSelector;
            Values.CreateNoteMode.OnValueChange -= UpdateModePalleteVisual;
            Values.CreateArcTypeMode.OnValueChange -= UpdateArcTypeVisual;
            Values.CreateArcColorMode.OnValueChange -= UpdateArcColorVisual;
        }

        private void UpdateModePalleteVisual(CreateNoteMode mode)
        {
            tapButtonHighlight.SetActive(mode == CreateNoteMode.Tap);
            holdButtonHighlight.SetActive(mode == CreateNoteMode.Hold);
            arcButtonHighlight.SetActive(mode == CreateNoteMode.Arc);
            traceButtonHighlight.SetActive(mode == CreateNoteMode.Trace);
            arctapButtonHighlight.SetActive(mode == CreateNoteMode.ArcTap);
            Services.Cursor.EnableLaneCursor = mode != CreateNoteMode.Idle;
        }

        private void UpdateArcTypeVisual(ArcLineType type)
        {
            arcTypeSelector.SetValueWithoutNotify(type);
        }

        private void UpdateArcColorVisual(int color)
        {
            arcColorSelector.SetValueWithoutNotify(color);
            arcImage.color = arcColorSelector.PreviewColor;
        }

        private void OnTapButton()
        {
            if (Values.CreateNoteMode.Value == CreateNoteMode.Tap)
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Idle;
                return;
            }

            Values.CreateNoteMode.Value = CreateNoteMode.Tap;
        }

        private void OnHoldButton()
        {
            if (Values.CreateNoteMode.Value == CreateNoteMode.Hold)
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Idle;
                return;
            }

            Values.CreateNoteMode.Value = CreateNoteMode.Hold;
        }

        private void OnArcButton()
        {
            if (Values.CreateNoteMode.Value == CreateNoteMode.Arc)
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Idle;
                return;
            }

            Values.CreateNoteMode.Value = CreateNoteMode.Arc;
        }

        private void OnTraceButton()
        {
            if (Values.CreateNoteMode.Value == CreateNoteMode.Trace)
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Idle;
                return;
            }

            Values.CreateNoteMode.Value = CreateNoteMode.Trace;
        }

        private void OnArcTapButton()
        {
            if (Values.CreateNoteMode.Value == CreateNoteMode.ArcTap)
            {
                Values.CreateNoteMode.Value = CreateNoteMode.Idle;
                return;
            }

            Values.CreateNoteMode.Value = CreateNoteMode.ArcTap;
        }

        private void OnArcColorSelector(int color)
        {
            Values.CreateArcColorMode.Value = color;
        }

        private void OnArcTypeSelector(ArcLineType type)
        {
            Values.CreateArcTypeMode.Value = type;
        }
    }
}