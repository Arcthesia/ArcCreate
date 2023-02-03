using System.Collections.Generic;
using System.Linq;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public class VerticalGrid : MonoBehaviour
    {
        public static readonly Rect DefaultArea = new Rect(-0.5f, -0.2f, 2, 1);
        public const float DefaultSnapTolerance = 1f;

        [SerializeField] private Color defaultLineColor;
        [SerializeField] private Color defaultPanelColor;
        [SerializeField] private MeshCollider verticalCollider;
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

        public static Color DefaultLineColor { get; private set; }

        public static Color DefaultPanelColor { get; private set; }

        public void LoadGridSettings(Rect area, Color panelColor, float snapTolerance, List<Line> lines, List<Area> areas)
        {
            this.lines = lines.Where(def => def.Interactable).ToList();
            decorativeLines = lines.Where(def => !def.Interactable).ToList();
            cachedIntersections = VerticalGridHelper.PrecalculateIntersections(this.lines);

            this.snapTolerance = snapTolerance;

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            int shaderId = Shader.PropertyToID("_Color");
            verticalPanelRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(shaderId, panelColor);
            verticalPanelRenderer.SetPropertyBlock(mpb);

            ResizeCollider(area);
            DrawLines();
            GenerateAreas(areas);
        }

        public void SetGridEnabled(bool enabled)
        {
            gridParent.gameObject.SetActive(enabled);
        }

        public Vector2 SnapToVerticalGrid(Vector2 point)
        {
            return VerticalGridHelper.SnapPoint(lines, cachedIntersections, point, snapTolerance);
        }

        private void ResizeCollider(Rect area)
        {
            Destroy(verticalCollider.sharedMesh);
            verticalCollider.sharedMesh = MeshBuilder.BuildQuadMeshVertical(area);
            verticalPanel.sharedMesh = verticalCollider.sharedMesh;
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

        private void Awake()
        {
            DefaultLineColor = defaultLineColor;
            DefaultPanelColor = defaultPanelColor;
            linePool = Pools.New<LineRenderer>(linePrefab.name, linePrefab, gridParent, 10);

            verticalCollider.sharedMesh = Instantiate(verticalCollider.sharedMesh);
            verticalPanel.sharedMesh = verticalCollider.sharedMesh;

            verticalPanelRenderer.sharedMaterial = Instantiate(verticalPanelRenderer.sharedMaterial);
        }

        private void Draw(Line def)
        {
            LineRenderer line = linePool.Get();
            line.DrawLine(def.Start, def.End);
            line.startColor = def.Color;
            line.endColor = def.Color;
        }
    }
}