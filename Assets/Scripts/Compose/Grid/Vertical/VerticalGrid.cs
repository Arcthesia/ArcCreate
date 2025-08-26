using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public class VerticalGrid : MonoBehaviour
    {
        public static readonly Rect DefaultArea = new Rect(-0.5f, -0.2f, 2, 1);
        public const float DefaultSnapTolerance = 1f;
        public const float DefaultIncrementX = 0.125f;
        public const float DefaultIncrementY = 0.25f;

        [SerializeField] private Color defaultLineColor;
        [SerializeField] private Color defaultPanelColor;
        [SerializeField] private MeshFilter verticalPanel;
        [SerializeField] private MeshRenderer verticalPanelRenderer;
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private GameObject areaPrefab;
        private Pool<LineRenderer> linePool;

        private List<Line> lines;
        private List<Line> decorativeLines;
        private List<Vector2> cachedIntersections;
        private readonly List<MeshFilter> areaMeshes = new List<MeshFilter>();
        private float snapTolerance;
        private bool scaleGridToSkyInput;
        private float verticalScale = 1;
        private Rect area;

        public static Color DefaultLineColor { get; private set; }

        public static Color DefaultPanelColor { get; private set; }

        public void LoadGridSettings(Rect area, Color panelColor, float snapTolerance, List<Line> lines, List<Area> areas, bool scaleGridToSkyInput)
        {
            this.area = area;
            this.lines = lines.Where(def => def.Interactable).ToList();
            decorativeLines = lines.Where(def => !def.Interactable).ToList();
            cachedIntersections = VerticalGridHelper.PrecalculateIntersections(this.lines);

            this.snapTolerance = snapTolerance;
            this.scaleGridToSkyInput = scaleGridToSkyInput;

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            int shaderId = Shader.PropertyToID("_Color");
            verticalPanelRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(shaderId, panelColor);
            verticalPanelRenderer.SetPropertyBlock(mpb);

            ResizePanel(area);
            DrawLines();
            GenerateAreas(areas);
        }

        public void SetGridEnabled(bool enabled)
        {
            gridParent.gameObject.SetActive(enabled);
            this.enabled = enabled;
        }

        public Vector2 SnapToVerticalGrid(Vector2 point)
        {
            point.y /= verticalScale;
            Vector2 snap = VerticalGridHelper.SnapPoint(lines, cachedIntersections, point, snapTolerance);
            snap.x = Mathf.Round(snap.x * 1000) / 1000;
            snap.y = Mathf.Round(snap.y * 1000) / 1000 * verticalScale;
            return snap;
        }

        public void Setup()
        {
            DefaultLineColor = defaultLineColor;
            DefaultPanelColor = defaultPanelColor;
            linePool = Pools.New<LineRenderer>(linePrefab.name, linePrefab, gridParent, 10);

            verticalPanel.sharedMesh = Instantiate(verticalPanel.sharedMesh);
            verticalPanelRenderer.sharedMaterial = Instantiate(verticalPanelRenderer.sharedMaterial);
        }

        public (float fromX, float fromY, float toX, float toY) GetBounds()
        {
            // idk why i have to do this.
            return (Math.Min(area.xMin, area.xMax),
                    Math.Min(area.yMin, area.yMax) * verticalScale,
                    Math.Max(area.xMax, area.xMin),
                    Math.Max(area.yMax, area.yMin) * verticalScale);
        }

        private void ResizePanel(Rect area)
        {
            Destroy(verticalPanel.sharedMesh);
            verticalPanel.sharedMesh = MeshBuilder.BuildQuadMeshVertical(area);
            verticalPanel.sharedMesh = verticalPanel.sharedMesh;
        }

        private void DrawLines()
        {
            linePool.ReturnAll();
            foreach (var line in lines)
            {
                Draw(line);
            }

            foreach (var line in decorativeLines)
            {
                Draw(line);
            }
        }

        private void GenerateAreas(List<Area> areas)
        {
            foreach (var meshFilter in areaMeshes)
            {
                Destroy(meshFilter.sharedMesh);
            }

            areaMeshes.Clear();
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            int shaderId = Shader.PropertyToID("_Color");

            foreach (Area area in areas)
            {
                Mesh mesh = MeshBuilder.BuildGenericVerticalMesh(area.Points);
                Color color = area.Color;

                GameObject go = Instantiate(areaPrefab, gridParent);
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();

                renderer.GetPropertyBlock(mpb);
                mpb.SetColor(shaderId, color);
                renderer.SetPropertyBlock(mpb);

                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                areaMeshes.Add(meshFilter);
            }
        }

        private void Draw(Line def)
        {
            LineRenderer line = linePool.Get();
            line.DrawLine(def.Start, def.End);
            line.startColor = def.Color;
            line.endColor = def.Color;
        }

        private void Update()
        {
            if (scaleGridToSkyInput)
            {
                ValueChannel skyInputY = Services.Gameplay.Scenecontrol.Scene.GetSkyInputYChannel();
                var tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
                int timing = tg.GetTimingFromZPosition(verticalPanel.transform.localPosition.z);
                verticalScale = skyInputY.ValueAt(timing) / Gameplay.Values.ArcY1;
                verticalPanel.transform.localScale = new Vector3(1, verticalScale, 0.001f);
            }
            else
            {
                verticalPanel.transform.localScale = new Vector3(1, 1, 0.001f);
                verticalScale = 1;
            }
        }
    }
}