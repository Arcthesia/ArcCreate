using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class ArcSegment : MonoBehaviour
    {
        private static readonly int FromShaderId = Shader.PropertyToID("_From");
        private static readonly int ShearShaderId = Shader.PropertyToID("_Shear");
        private static readonly int ColorShaderId = Shader.PropertyToID("_ColorTG");
        private static readonly int SelectedShaderId = Shader.PropertyToID("_Selected");
        private static MaterialPropertyBlock mpb;
        [SerializeField] private MeshRenderer shadowMeshRenderer;
        [SerializeField] private MeshFilter shadowMeshFilter;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        public int Timing { get; set; }

        public int EndTiming { get; set; }

        public Vector3 StartPosition { get; set; }

        public Vector3 EndPosition { get; set; }

        public double FloorPosition { get; set; }

        public double EndFloorPosition { get; set; }

        public void SetColor(Color color)
        {
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorShaderId, color);
            meshRenderer.SetPropertyBlock(mpb);

            shadowMeshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(ColorShaderId, color);
            shadowMeshRenderer.SetPropertyBlock(mpb);
        }

        public void SetFrom(float from)
        {
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(FromShaderId, from);
            meshRenderer.SetPropertyBlock(mpb);

            shadowMeshRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(FromShaderId, from);
            shadowMeshRenderer.SetPropertyBlock(mpb);

            gameObject.SetActive(from < 1);
        }

        public void SetMaterial(Material material, Material shadow)
        {
            meshRenderer.sharedMaterial = material;
            shadowMeshRenderer.sharedMaterial = shadow;
        }

        public void SetMesh(Mesh mesh, Mesh shadow)
        {
            meshFilter.sharedMesh = mesh;
            shadowMeshFilter.sharedMesh = shadow;
        }

        public void SetSelected(bool value)
        {
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetInt(SelectedShaderId, value ? 1 : 0);
            meshRenderer.SetPropertyBlock(mpb);
        }

        public void UpdatePosition(double floorPosition, Vector3 fallDirection, float parentZ)
        {
            float startZ = ArcFormula.FloorPositionToZ(FloorPosition - floorPosition);
            float endZ = ArcFormula.FloorPositionToZ(EndFloorPosition - floorPosition);
            Vector3 startPos = StartPosition + ((startZ - parentZ) * fallDirection);
            Vector3 endPos = EndPosition + ((endZ - parentZ) * fallDirection);
            Vector3 dir = endPos - startPos;

            transform.localPosition = startPos;
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetVector(ShearShaderId, new Vector4(
                dir.x,
                dir.y,
                dir.z,
                0));
            meshRenderer.SetPropertyBlock(mpb);

            shadowMeshRenderer.GetPropertyBlock(mpb);
            mpb.SetVector(ShearShaderId, new Vector4(
                dir.x,
                dir.y,
                dir.z,
                0));
            shadowMeshRenderer.SetPropertyBlock(mpb);

            gameObject.SetActive(startZ >= -Values.TrackLengthForward && endZ <= Values.TrackLengthBackward);
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            mpb = new MaterialPropertyBlock();
            meshRenderer.sortingLayerName = "Arc";
            meshRenderer.sortingOrder = 1;
            shadowMeshRenderer.sortingLayerName = "Arc";
            shadowMeshRenderer.sortingOrder = 0;
        }
    }
}