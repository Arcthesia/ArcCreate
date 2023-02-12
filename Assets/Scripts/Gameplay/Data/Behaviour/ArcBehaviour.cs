using System.Collections.Generic;
using ArcCreate.Gameplay.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class ArcBehaviour : NoteBehaviour
    {
        // No matter what codebase I work in this will always be a nightmare
        private static readonly int ColorShaderId = Shader.PropertyToID("_ColorTG");
        private static readonly int SelectedShaderId = Shader.PropertyToID("_Selected");
        private static Pool<ArcSegment> segmentPool;
        private static MaterialPropertyBlock mpb;

        [SerializeField] private SpriteRenderer arcCap;
        [SerializeField] private SpriteRenderer heightIndicator;
        [SerializeField] private MeshRenderer arcHeadRenderer;
        [SerializeField] private MeshFilter arcHeadMesh;
        [SerializeField] private MeshCollider meshCollider;
        private Material normalMaterial;
        private Material highlightMaterial;
        private Material shadowMaterial;
        private bool highlight;
        private readonly List<ArcSegment> segments = new List<ArcSegment>(32);

        public Arc Arc { get; private set; }

        public override Note Note => Arc;

        public bool Highlight
        {
            get => highlight;
            set
            {
                highlight = value;
                Material m = value ? highlightMaterial : normalMaterial;

                for (int i = 0; i < segments.Count; i++)
                {
                    ArcSegment segment = segments[i];
                    segment.SetMaterial(m, shadowMaterial);
                }

                arcHeadRenderer.sharedMaterial = m;
            }
        }

        private Material Material => Highlight ? highlightMaterial : normalMaterial;

        public void SetData(Arc arc)
        {
            Arc = arc;
            arcCap.color = new Color(1, 1, 1, 0);
            segmentPool = segmentPool ?? Pools.Get<ArcSegment>(Values.ArcSegmentPoolName);
            arcHeadMesh.sharedMesh = ArcMeshGenerator.GetHeadMesh(arc);
            arcHeadRenderer.sharedMaterial = Material;
        }

        public void RebuildSegments()
        {
            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegment segment = segments[i];
                segmentPool.Return(segment);
            }

            segments.Clear();

            int lastEndTiming = Arc.Timing;
            double lastEndFloorPosition = Arc.TimingGroupInstance.GetFloorPosition(Arc.Timing);
            Vector2 basePosition = new Vector2(ArcFormula.ArcXToWorld(Arc.XStart), ArcFormula.ArcYToWorld(Arc.YStart));
            Vector2 lastPosition = basePosition;

            while (true)
            {
                int timing = lastEndTiming;
                int endTiming = timing + Mathf.RoundToInt(Arc.SegmentLength);
                int cappedEndTiming = Mathf.Min(endTiming, Arc.EndTiming);

                ArcSegment newSegment = segmentPool.Get(transform);

                newSegment.Timing = timing;
                newSegment.EndTiming = cappedEndTiming;
                newSegment.FloorPosition = lastEndFloorPosition;
                newSegment.StartPosition = lastPosition - basePosition;

                lastEndFloorPosition = Arc.TimingGroupInstance.GetFloorPosition(cappedEndTiming);
                newSegment.EndFloorPosition = lastEndFloorPosition;
                lastPosition = cappedEndTiming == Arc.EndTiming ?
                    new Vector2(ArcFormula.ArcXToWorld(Arc.XEnd), ArcFormula.ArcYToWorld(Arc.YEnd)) :
                    new Vector2(Arc.WorldXAt(cappedEndTiming), Arc.WorldYAt(cappedEndTiming));
                newSegment.EndPosition = lastPosition - basePosition;

                newSegment.SetMaterial(Material, shadowMaterial);
                newSegment.SetFrom(0);
                newSegment.SetMesh(ArcMeshGenerator.GetSegmentMesh(Arc), ArcMeshGenerator.GetShadowMesh(Arc));
                segments.Add(newSegment);

                lastEndTiming = endTiming;
                if (endTiming >= Arc.EndTiming)
                {
                    break;
                }
            }
        }

        public void SetSkin(Material normal, Material highlight, Material shadow, Sprite arcCapSprite, Sprite heightIndicatorSprite, Color heightIndicatorColor)
        {
            arcCap.sprite = arcCapSprite;
            heightIndicator.color = heightIndicatorColor;
            normalMaterial = normal;
            highlightMaterial = highlight;
            heightIndicator.sprite = heightIndicatorSprite;
            shadowMaterial = shadow;
            arcHeadRenderer.sharedMaterial = Material;

            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegment segment = segments[i];
                segment.SetMaterial(Material, shadow);
            }
        }

        public void SetColor(Color color)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegment segment = segments[i];
                segment.SetColor(color);
            }

            arcHeadRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorShaderId, color);
            arcHeadRenderer.SetPropertyBlock(mpb);
        }

        public void SetSelected(bool value)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegment segment = segments[i];
                segment.SetSelected(value);
            }

            arcHeadRenderer.GetPropertyBlock(mpb);
            mpb.SetInt(SelectedShaderId, value ? 1 : 0);
            arcHeadRenderer.SetPropertyBlock(mpb);

            heightIndicator.GetPropertyBlock(mpb);
            mpb.SetInt(SelectedShaderId, value ? 1 : 0);
            heightIndicator.SetPropertyBlock(mpb);
        }

        public void SetCollider(Mesh mesh)
        {
            meshCollider.sharedMesh = mesh;
        }

        public void ClipTo(int currentTiming, double currentFloorPosition, int clipToTiming, double clipToFloorPosition)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegment segment = segments[i];
                if (clipToTiming >= segment.EndTiming && currentTiming >= segment.EndTiming)
                {
                    segment.SetFrom(1);
                }
                else if (clipToTiming <= segment.Timing)
                {
                    segment.SetFrom(0);
                }
                else
                {
                    float p = (float)((clipToFloorPosition - segment.FloorPosition) / (segment.EndFloorPosition - segment.FloorPosition));
                    segment.SetFrom(p);
                }

                if (currentTiming >= segment.Timing && currentTiming < segment.EndTiming)
                {
                    float p = (float)((currentFloorPosition - segment.FloorPosition) / (segment.EndFloorPosition - segment.FloorPosition));
                    Vector3 capPos = segment.StartPosition + (p * (segment.EndPosition - segment.StartPosition));
                    capPos.z = -transform.localPosition.z;
                    arcCap.transform.localPosition = capPos;

                    arcCap.transform.localScale = new Vector3(Arc.ArcCapSize, Arc.ArcCapSize, 1);
                    arcCap.color = new Color(1, 1, 1, Arc.IsTrace ? Values.TraceCapAlpha : Values.ArcCapAlpha);
                }
            }

            if (currentTiming <= Arc.Timing && Arc.IsFirstArcOfGroup)
            {
                float z = -transform.localPosition.z;
                float approach = 1 - (Mathf.Abs(z) / Values.TrackLengthForward);
                arcCap.transform.localPosition = new Vector3(0, 0, z);
                arcCap.color = new Color(1, 1, 1, Arc.IsTrace ? 0 : approach);
                if (!Arc.IsTrace)
                {
                    float size = Values.ArcCapSize + (Values.ArcCapSizeAdditionMax * (1 - approach));
                    arcCap.transform.localScale = new Vector3(size, size, 1);
                }
            }

            if (currentTiming >= Arc.EndTiming)
            {
                arcCap.color = new Color(1, 1, 1, 0);
            }

            heightIndicator.enabled = clipToTiming <= Arc.Timing && Arc.ShouldDrawHeightIndicator;
            arcHeadRenderer.enabled = clipToTiming <= Arc.Timing && Arc.IsFirstArcOfGroup;
        }

        public void SetTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
            transform.localScale = scale;

            Vector3 heightScale = heightIndicator.transform.localScale;
            heightScale.y = -100 * (position.y - (Values.TraceMeshOffset / 2));
            heightIndicator.transform.localScale = heightScale;
        }

        public void UpdateSegmentsPosition(double floorPosition, Vector3 fallDirection)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                ArcSegment segment = segments[i];
                segment.UpdatePosition(floorPosition, fallDirection, transform.localPosition.z);
            }
        }

        private void Awake()
        {
            mpb = new MaterialPropertyBlock();
            arcHeadRenderer.sortingLayerName = "Arc";
            arcHeadRenderer.sortingOrder = 6;
        }
    }
}