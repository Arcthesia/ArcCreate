using System.Collections.Generic;
using ArcCreate.Compose.Components;
using ArcCreate.Utility.Extension;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class ChartColorsUI : ChartMetadataUI
    {
        [SerializeField] private float debounceSeconds = 0.5f;
        [SerializeField] private ColorInputField traceBody;
        [SerializeField] private ColorInputField shadow;
        [SerializeField] private ArcColorInputField arcBlue;
        [SerializeField] private ArcColorInputField arcRed;
        [SerializeField] private ArcColorInputField arcGreen;
        [SerializeField] private List<ArcColorInputField> arcCustom;
        [SerializeField] private Button newArcColorButton;
        [SerializeField] private Transform arcParent;
        [SerializeField] private GameObject colorInputFieldPrefab;
        private float applyAfter = float.MaxValue;

        protected override void ApplyChartSettings(ChartSettings chart)
        {
            foreach (var arc in arcCustom)
            {
                arc.OnValueChange -= Schedule;
                Destroy(arc.gameObject);
            }

            var defaultArc = Services.Gameplay.Skin.DefaultArcColors;
            var defaultArcLow = Services.Gameplay.Skin.DefaultArcLowColors;
            var defaultTrace = Services.Gameplay.Skin.DefaultTraceColor;
            var defaultShadow = Services.Gameplay.Skin.DefaultShadowColor;

            if (chart.Colors == null)
            {
                arcBlue.SetValueWithoutNotify(defaultArc[0], defaultArcLow[0]);
                arcRed.SetValueWithoutNotify(defaultArc[1], defaultArcLow[1]);
                arcGreen.SetValueWithoutNotify(defaultArc[2], defaultArcLow[2]);
                traceBody.SetValueWithoutNotify(defaultTrace);
                shadow.SetValueWithoutNotify(defaultShadow);
                return;
            }

            chart.Colors.Trace.ConvertHexToColor(out Color trace);
            chart.Colors.Shadow.ConvertHexToColor(out Color sdw);
            traceBody.SetValueWithoutNotify(trace);
            shadow.SetValueWithoutNotify(sdw);

            int count = Mathf.Min(chart.Colors.Arc.Count, chart.Colors.ArcLow.Count);
            for (int i = 3; i < chart.Colors.Arc.Count; i++)
            {
                chart.Colors.Arc[i].ConvertHexToColor(out Color high);
                chart.Colors.ArcLow[i].ConvertHexToColor(out Color low);
                NewArcColor(high, low);
            }

            if (count <= 0)
            {
                arcBlue.SetValueWithoutNotify(defaultArc[0], defaultArcLow[0]);
            }
            else
            {
                chart.Colors.Arc[0].ConvertHexToColor(out Color high);
                chart.Colors.ArcLow[0].ConvertHexToColor(out Color low);
                arcBlue.SetValueWithoutNotify(high, low);
            }

            if (count <= 1)
            {
                arcRed.SetValueWithoutNotify(defaultArc[1], defaultArcLow[1]);
            }
            else
            {
                chart.Colors.Arc[1].ConvertHexToColor(out Color high);
                chart.Colors.ArcLow[1].ConvertHexToColor(out Color low);
                arcRed.SetValueWithoutNotify(high, low);
            }

            if (count <= 2)
            {
                arcRed.SetValueWithoutNotify(defaultArc[2], defaultArc[2]);
            }
            else
            {
                chart.Colors.Arc[2].ConvertHexToColor(out Color high);
                chart.Colors.ArcLow[2].ConvertHexToColor(out Color low);
            }
        }

        private new void Start()
        {
            base.Start();
            traceBody.OnValueChange += Schedule;
            shadow.OnValueChange += Schedule;
            shadow.OnValueChange += Schedule;
            arcRed.OnValueChange += Schedule;
            arcGreen.OnValueChange += Schedule;
            arcBlue.OnValueChange += Schedule;

            newArcColorButton.onClick.AddListener(OnNewArcColorButton);
        }

        private void OnDestroy()
        {
            traceBody.OnValueChange -= Schedule;
            shadow.OnValueChange -= Schedule;
            shadow.OnValueChange -= Schedule;
            arcRed.OnValueChange -= Schedule;
            arcGreen.OnValueChange -= Schedule;
            arcBlue.OnValueChange -= Schedule;

            foreach (var arc in arcCustom)
            {
                arc.OnValueChange -= Schedule;
            }

            newArcColorButton.onClick.RemoveListener(OnNewArcColorButton);
        }

        private void Schedule(Color obj)
        {
            applyAfter = Time.realtimeSinceStartup + debounceSeconds;
        }

        private void Schedule((Color obj, Color obj1) arcColors)
        {
            applyAfter = Time.realtimeSinceStartup + debounceSeconds;
        }

        private void OnNewArcColorButton()
        {
            NewArcColor(Services.Gameplay.Skin.UnknownArcColor, Services.Gameplay.Skin.UnknownArcLowColor);
        }

        private void NewArcColor(Color high, Color low)
        {
            GameObject go = Instantiate(colorInputFieldPrefab, arcParent);
            var comp = go.GetComponent<ArcColorInputField>();
            comp.OnValueChange += Schedule;
            comp.SetValueWithoutNotify(high, low);
            arcCustom.Add(comp);
        }

        private void ApplyColorSettings()
        {
            Services.Gameplay.Skin.SetTraceColor(traceBody.Value, shadow.Value);

            List<Color> highColors = new List<Color>()
            {
                arcBlue.High,
                arcRed.High,
                arcGreen.High,
            };

            List<Color> lowColors = new List<Color>()
            {
                arcBlue.Low,
                arcRed.Low,
                arcGreen.Low,
            };

            List<string> highColorStrings = new List<string>()
            {
                arcBlue.High.ConvertToHexCode(),
                arcRed.High.ConvertToHexCode(),
                arcGreen.High.ConvertToHexCode(),
            };

            List<string> lowColorStrings = new List<string>()
            {
                arcBlue.Low.ConvertToHexCode(),
                arcRed.Low.ConvertToHexCode(),
                arcGreen.Low.ConvertToHexCode(),
            };

            foreach (ArcColorInputField arc in arcCustom)
            {
                highColors.Add(arc.High);
                lowColors.Add(arc.Low);
                highColorStrings.Add(arc.High.ConvertToHexCode());
                lowColorStrings.Add(arc.Low.ConvertToHexCode());
            }

            Services.Gameplay.Skin.SetTraceColor(traceBody.Value, shadow.Value);
            Services.Gameplay.Skin.SetArcColors(highColors, lowColors, shadow.Value);

            Target.Colors = Target.Colors ?? new ColorSettings();
            Target.Colors.Trace = traceBody.Value.ConvertToHexCode();
            Target.Colors.Shadow = shadow.Value.ConvertToHexCode();
            Target.Colors.Arc = highColorStrings;
            Target.Colors.ArcLow = lowColorStrings;
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup >= applyAfter)
            {
                ApplyColorSettings();
                applyAfter = float.MaxValue;
            }
        }
    }
}