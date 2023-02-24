using System.Collections.Generic;
using ArcCreate.Gameplay.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public class RenderService : MonoBehaviour, IRenderService
    {
        [SerializeField] private Camera notesCamera;
        [SerializeField] private int layer;

        [Header("Meshes")]
        [SerializeField] private Mesh arctapMesh;
        [SerializeField] private Mesh arctapSfxMesh;
        [SerializeField] private Mesh arctapShadowMesh;
        [SerializeField] private Mesh tapMesh;
        [SerializeField] private Mesh holdMesh;
        [SerializeField] private Mesh connectionLineMesh;
        [SerializeField] private Mesh heightIndicatorMesh;
        [SerializeField] private Mesh arcCapMesh;

        [Header("Materials")]
        [SerializeField] private Material baseArctapMaterial;
        [SerializeField] private Material baseTapMaterial;
        [SerializeField] private Material baseHoldMaterial;
        [SerializeField] private Material baseArcCapMaterial;
        [SerializeField] private Material arctapShadowMaterial;
        [SerializeField] private Material heightIndicatorMaterial;
        [SerializeField] private Material connectionLineMaterial;

        private readonly List<Material> generatedMaterials = new List<Material>();

        // Floor
        private readonly Dictionary<Texture, InstancedRendererPool<NoteRenderProperties>> tapDrawers
            = new Dictionary<Texture, InstancedRendererPool<NoteRenderProperties>>();

        private readonly Dictionary<Texture, InstancedRendererPool<LongNoteRenderProperties>> holdDrawers
            = new Dictionary<Texture, InstancedRendererPool<LongNoteRenderProperties>>();

        private InstancedRendererPool<SpriteRenderProperties> connectionLineDrawer;

        // Arc & trace
        private readonly List<InstancedRendererPool<ArcRenderProperties>> arcSegmentDrawers
            = new List<InstancedRendererPool<ArcRenderProperties>>();

        private readonly List<InstancedRendererPool<ArcRenderProperties>> arcHeadDrawers
            = new List<InstancedRendererPool<ArcRenderProperties>>();

        private readonly List<InstancedRendererPool<ArcRenderProperties>> arcSegmentHighlightDrawers
            = new List<InstancedRendererPool<ArcRenderProperties>>();

        private readonly List<InstancedRendererPool<ArcRenderProperties>> arcHeadHighlightDrawers
            = new List<InstancedRendererPool<ArcRenderProperties>>();

        private readonly Dictionary<Texture, InstancedRendererPool<SpriteRenderProperties>> arcCapDrawers
            = new Dictionary<Texture, InstancedRendererPool<SpriteRenderProperties>>();

        private InstancedRendererPool<SpriteRenderProperties> arcShadowDrawer;
        private InstancedRendererPool<ArcRenderProperties> traceSegmentDrawer;
        private InstancedRendererPool<ArcRenderProperties> traceHeadDrawer;
        private InstancedRendererPool<SpriteRenderProperties> traceShadowDrawer;
        private InstancedRendererPool<SpriteRenderProperties> heightIndicatorDrawer;

        // Arctap
        private readonly Dictionary<Texture, InstancedRendererPool<NoteRenderProperties>> arctapDrawers
            = new Dictionary<Texture, InstancedRendererPool<NoteRenderProperties>>();

        private readonly Dictionary<Texture, InstancedRendererPool<NoteRenderProperties>> arctapSfxDrawers
            = new Dictionary<Texture, InstancedRendererPool<NoteRenderProperties>>();

        private InstancedRendererPool<SpriteRenderProperties> arctapShadowDrawer;

        public bool IsLoaded { get; private set; }

        public Mesh TapMesh => tapMesh;

        public Mesh HoldMesh => holdMesh;

        public Mesh ArcTapMesh => arctapMesh;

        public void DrawTap(Texture texture, Matrix4x4 matrix, NoteRenderProperties properties)
        {
            if (!tapDrawers.ContainsKey(texture))
            {
                Material newTap = Instantiate(baseTapMaterial);
                newTap.mainTexture = texture;
                generatedMaterials.Add(newTap);
                tapDrawers.Add(texture, new InstancedRendererPool<NoteRenderProperties>(
                    newTap,
                    tapMesh,
                    Shader.PropertyToID("_Properties"),
                    NoteRenderProperties.Size()));
            }

            tapDrawers[texture].RegisterInstance(matrix, properties);
        }

        public void DrawHold(Texture texture, Matrix4x4 matrix, LongNoteRenderProperties properties)
        {
            if (!holdDrawers.ContainsKey(texture))
            {
                Material newHold = Instantiate(baseHoldMaterial);
                newHold.mainTexture = texture;
                generatedMaterials.Add(newHold);
                holdDrawers.Add(texture, new InstancedRendererPool<LongNoteRenderProperties>(
                    newHold,
                    holdMesh,
                    Shader.PropertyToID("_Properties"),
                    LongNoteRenderProperties.Size()));
            }

            holdDrawers[texture].RegisterInstance(matrix, properties);
        }

        public void DrawConnectionLine(Matrix4x4 matrix, SpriteRenderProperties properties)
        {
            connectionLineDrawer.RegisterInstance(matrix, properties);
        }

        public void DrawArcSegment(int colorId, bool highlight, Matrix4x4 matrix, ArcRenderProperties properties)
        {
            colorId = Mathf.Min(colorId, arcSegmentDrawers.Count - 1);

            if (highlight)
            {
                arcSegmentHighlightDrawers[colorId].RegisterInstance(matrix, properties);
            }
            else
            {
                arcSegmentDrawers[colorId].RegisterInstance(matrix, properties);
            }
        }

        public void DrawTraceSegment(Matrix4x4 matrix, ArcRenderProperties properties)
        {
            traceSegmentDrawer.RegisterInstance(matrix, properties);
        }

        public void DrawArcShadow(Matrix4x4 matrix, SpriteRenderProperties properties)
        {
            arcShadowDrawer.RegisterInstance(matrix, properties);
        }

        public void DrawTraceShadow(Matrix4x4 matrix, SpriteRenderProperties properties)
        {
            traceShadowDrawer.RegisterInstance(matrix, properties);
        }

        public void DrawArcHead(int colorId, bool highlight, Matrix4x4 matrix, ArcRenderProperties properties)
        {
            colorId = Mathf.Min(colorId, arcHeadDrawers.Count - 1);

            if (highlight)
            {
                arcHeadHighlightDrawers[colorId].RegisterInstance(matrix, properties);
            }
            else
            {
                arcHeadDrawers[colorId].RegisterInstance(matrix, properties);
            }
        }

        public void DrawTraceHead(Matrix4x4 matrix, ArcRenderProperties properties)
        {
            traceHeadDrawer.RegisterInstance(matrix, properties);
        }

        public void DrawArcCap(Texture texture, Matrix4x4 matrix, SpriteRenderProperties properties)
        {
            if (!arcCapDrawers.ContainsKey(texture))
            {
                Material newArcCap = Instantiate(baseArcCapMaterial);
                newArcCap.mainTexture = texture;
                generatedMaterials.Add(newArcCap);
                arcCapDrawers.Add(texture, new InstancedRendererPool<SpriteRenderProperties>(
                    newArcCap,
                    arcCapMesh,
                    Shader.PropertyToID("_Properties"),
                    SpriteRenderProperties.Size()));
            }

            arcCapDrawers[texture].RegisterInstance(matrix, properties);
        }

        public void DrawHeightIndicator(Matrix4x4 matrix, SpriteRenderProperties properties)
        {
            heightIndicatorDrawer.RegisterInstance(matrix, properties);
        }

        public void DrawArcTap(bool sfx, Texture texture, Matrix4x4 matrix, NoteRenderProperties properties)
        {
            var drawer = sfx ? arctapSfxDrawers : arctapDrawers;
            if (!drawer.ContainsKey(texture))
            {
                Material newArctap = Instantiate(baseArctapMaterial);
                newArctap.mainTexture = texture;
                generatedMaterials.Add(newArctap);
                drawer.Add(texture, new InstancedRendererPool<NoteRenderProperties>(
                    newArctap,
                    sfx ? arctapSfxMesh : arctapMesh,
                    Shader.PropertyToID("_Properties"),
                    NoteRenderProperties.Size()));
            }

            drawer[texture].RegisterInstance(matrix, properties);
        }

        public void DrawArcTapShadow(Matrix4x4 matrix, SpriteRenderProperties properties)
        {
            arctapShadowDrawer.RegisterInstance(matrix, properties);
        }

        public void UpdateRenderers()
        {
            foreach (var pair in holdDrawers)
            {
                pair.Value.Draw(notesCamera, layer);
            }

            connectionLineDrawer.Draw(notesCamera, layer);
            foreach (var pair in tapDrawers)
            {
                pair.Value.Draw(notesCamera, layer);
            }

            traceShadowDrawer.Draw(notesCamera, layer);
            arcShadowDrawer.Draw(notesCamera, layer);

            traceSegmentDrawer.Draw(notesCamera, layer);
            traceHeadDrawer.Draw(notesCamera, layer);
            arctapShadowDrawer.Draw(notesCamera, layer);

            heightIndicatorDrawer.Draw(notesCamera, layer);
            foreach (var pair in arcCapDrawers)
            {
                pair.Value.Draw(notesCamera, layer);
            }

            for (int i = 0; i < arcSegmentDrawers.Count; i++)
            {
                InstancedRendererPool<ArcRenderProperties> segment = arcSegmentDrawers[i];
                InstancedRendererPool<ArcRenderProperties> segmentHighlight = arcSegmentHighlightDrawers[i];
                segment.Draw(notesCamera, layer);
                segmentHighlight.Draw(notesCamera, layer);
            }

            foreach (var pair in arctapDrawers)
            {
                pair.Value.Draw(notesCamera, layer);
            }

            foreach (var pair in arctapSfxDrawers)
            {
                pair.Value.Draw(notesCamera, layer);
            }

            for (int i = 0; i < arcHeadDrawers.Count; i++)
            {
                InstancedRendererPool<ArcRenderProperties> head = arcHeadDrawers[i];
                InstancedRendererPool<ArcRenderProperties> headHighlight = arcHeadHighlightDrawers[i];
                head.Draw(notesCamera, layer);
                headHighlight.Draw(notesCamera, layer);
            }
        }

        public void SetTraceMaterial(Material material)
        {
            traceSegmentDrawer?.Dispose();
            traceSegmentDrawer = new InstancedRendererPool<ArcRenderProperties>(
                material,
                ArcMeshGenerator.GetSegmentMesh(true),
                Shader.PropertyToID("_Properties"),
                ArcRenderProperties.Size());

            traceHeadDrawer?.Dispose();
            traceHeadDrawer = new InstancedRendererPool<ArcRenderProperties>(
                material,
                ArcMeshGenerator.GetHeadMesh(true),
                Shader.PropertyToID("_Properties"),
                ArcRenderProperties.Size());

            UpdateLoadedState();
        }

        public void SetShadowMaterial(Material material)
        {
            arcShadowDrawer?.Dispose();
            arcShadowDrawer = new InstancedRendererPool<SpriteRenderProperties>(
                material,
                ArcMeshGenerator.GetShadowMesh(false),
                Shader.PropertyToID("_Properties"),
                SpriteRenderProperties.Size());

            traceShadowDrawer?.Dispose();
            traceShadowDrawer = new InstancedRendererPool<SpriteRenderProperties>(
                material,
                ArcMeshGenerator.GetShadowMesh(true),
                Shader.PropertyToID("_Properties"),
                SpriteRenderProperties.Size());

            UpdateLoadedState();
        }

        public void SetArcMaterials(List<Material> normal, List<Material> highlight)
        {
            for (int i = 0; i < arcSegmentDrawers.Count; i++)
            {
                arcSegmentDrawers[i].Dispose();
                arcHeadDrawers[i].Dispose();
            }

            arcSegmentDrawers.Clear();
            arcHeadDrawers.Clear();

            for (int i = 0; i < normal.Count; i++)
            {
                arcSegmentDrawers.Add(new InstancedRendererPool<ArcRenderProperties>(
                    normal[i],
                    ArcMeshGenerator.GetSegmentMesh(false),
                    Shader.PropertyToID("_Properties"),
                    ArcRenderProperties.Size()));

                arcHeadDrawers.Add(new InstancedRendererPool<ArcRenderProperties>(
                    normal[i],
                    ArcMeshGenerator.GetHeadMesh(false),
                    Shader.PropertyToID("_Properties"),
                    ArcRenderProperties.Size()));
            }

            for (int i = 0; i < arcSegmentHighlightDrawers.Count; i++)
            {
                arcSegmentHighlightDrawers[i].Dispose();
                arcHeadHighlightDrawers[i].Dispose();
            }

            arcSegmentHighlightDrawers.Clear();
            arcHeadHighlightDrawers.Clear();

            for (int i = 0; i < highlight.Count; i++)
            {
                arcSegmentHighlightDrawers.Add(new InstancedRendererPool<ArcRenderProperties>(
                    highlight[i],
                    ArcMeshGenerator.GetSegmentMesh(false),
                    Shader.PropertyToID("_Properties"),
                    ArcRenderProperties.Size()));

                arcHeadHighlightDrawers.Add(new InstancedRendererPool<ArcRenderProperties>(
                    highlight[i],
                    ArcMeshGenerator.GetHeadMesh(false),
                    Shader.PropertyToID("_Properties"),
                    ArcRenderProperties.Size()));
            }

            UpdateLoadedState();
        }

        public void SetTextures(Texture heightIndicator, Texture arctapShadow)
        {
            Material height = Instantiate(heightIndicatorMaterial);
            height.mainTexture = heightIndicator;
            generatedMaterials.Add(height);
            heightIndicatorDrawer = new InstancedRendererPool<SpriteRenderProperties>(
                height,
                heightIndicatorMesh,
                Shader.PropertyToID("_Properties"),
                SpriteRenderProperties.Size());

            Material shadow = Instantiate(arctapShadowMaterial);
            shadow.mainTexture = arctapShadow;
            generatedMaterials.Add(shadow);
            arctapShadowDrawer = new InstancedRendererPool<SpriteRenderProperties>(
                shadow,
                arctapShadowMesh,
                Shader.PropertyToID("_Properties"),
                SpriteRenderProperties.Size());

            UpdateLoadedState();
        }

        private void UpdateLoadedState()
        {
            IsLoaded = connectionLineDrawer != null
                    && arcSegmentDrawers.Count > 0
                    && arcHeadDrawers.Count > 0
                    && arcSegmentHighlightDrawers.Count > 0
                    && arcHeadHighlightDrawers.Count > 0
                    && traceSegmentDrawer != null
                    && traceHeadDrawer != null
                    && arcShadowDrawer != null
                    && traceShadowDrawer != null
                    && connectionLineDrawer != null
                    && heightIndicatorDrawer != null
                    && arctapShadowDrawer != null;
        }

        private void Awake()
        {
            connectionLineDrawer = new InstancedRendererPool<SpriteRenderProperties>(
                connectionLineMaterial,
                connectionLineMesh,
                Shader.PropertyToID("_Properties"),
                SpriteRenderProperties.Size());

            UpdateLoadedState();
        }

        private void OnDestroy()
        {
            foreach (Material material in generatedMaterials)
            {
                Destroy(material);
            }

            generatedMaterials.Clear();

            foreach (var pair in holdDrawers)
            {
                pair.Value.Dispose();
            }

            connectionLineDrawer.Dispose();
            foreach (var pair in tapDrawers)
            {
                pair.Value.Dispose();
            }

            traceShadowDrawer.Dispose();
            arcShadowDrawer.Dispose();

            traceSegmentDrawer.Dispose();
            traceHeadDrawer.Dispose();
            arctapShadowDrawer.Dispose();

            heightIndicatorDrawer.Dispose();
            foreach (var pair in arcCapDrawers)
            {
                pair.Value.Dispose();
            }

            for (int i = 0; i < arcSegmentDrawers.Count; i++)
            {
                InstancedRendererPool<ArcRenderProperties> segment = arcSegmentDrawers[i];
                InstancedRendererPool<ArcRenderProperties> segmentHighlight = arcSegmentHighlightDrawers[i];
                segment.Dispose();
                segmentHighlight.Dispose();
            }

            foreach (var pair in arctapDrawers)
            {
                pair.Value.Dispose();
            }

            foreach (var pair in arctapSfxDrawers)
            {
                pair.Value.Dispose();
            }

            for (int i = 0; i < arcHeadDrawers.Count; i++)
            {
                InstancedRendererPool<ArcRenderProperties> head = arcHeadDrawers[i];
                InstancedRendererPool<ArcRenderProperties> headHighlight = arcHeadHighlightDrawers[i];
                head.Dispose();
                headHighlight.Dispose();
            }
        }
    }
}