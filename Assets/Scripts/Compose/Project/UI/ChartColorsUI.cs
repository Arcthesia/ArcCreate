using System;
using System.Collections.Generic;
using ArcCreate.Compose.Components;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public class ChartColorsUI : ChartMetadataUI
    {
        [SerializeField] private float debounceSeconds = 1f;
        [SerializeField] private ColorInputField traceBody;
        [SerializeField] private ColorInputField shadow;
        [SerializeField] private ArcColorInputField arcBlue;
        [SerializeField] private ArcColorInputField arcRed;
        [SerializeField] private ArcColorInputField arcGreen;
        [SerializeField] private List<ArcColorInputField> arcCustom;
        [SerializeField] private Transform arcParent;
        [SerializeField] private GameObject colorInputFieldPrefab;
        private float applyAfter = float.MaxValue;

        protected override void ApplyChartSettings(ChartSettings chart)
        {
        }

        private new void Awake()
        {
            base.Awake();
            traceBody.OnValueChange += Schedule;
            shadow.OnValueChange += Schedule;
            shadow.OnValueChange += Schedule;
            arcRed.OnValueChange += Schedule;
            arcGreen.OnValueChange += Schedule;
            arcBlue.OnValueChange += Schedule;
        }

        private void OnDestroy()
        {
            traceBody.OnValueChange -= Schedule;
            shadow.OnValueChange -= Schedule;
            shadow.OnValueChange -= Schedule;
            arcRed.OnValueChange -= Schedule;
            arcGreen.OnValueChange -= Schedule;
            arcBlue.OnValueChange -= Schedule;
        }

        private void Schedule(Color obj)
        {
            applyAfter = Time.realtimeSinceStartup + debounceSeconds;
        }

        private void Schedule((Color obj, Color obj1) arcColors)
        {
            applyAfter = Time.realtimeSinceStartup + debounceSeconds;
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup >= applyAfter)
            {
            }
        }
    }
}