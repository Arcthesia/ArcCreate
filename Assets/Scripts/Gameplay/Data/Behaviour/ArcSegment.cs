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
        private static MaterialPropertyBlock mpb;

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
        }

        public void SetFrom(float from)
        {
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(FromShaderId, from);
            meshRenderer.SetPropertyBlock(mpb);

            gameObject.SetActive(from < 1);
        }

        public void SetMaterial(Material material)
        {
            meshRenderer.sharedMaterial = material;
        }

        public void SetMesh(Mesh mesh)
        {
            meshFilter.sharedMesh = mesh;
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

            gameObject.SetActive(startZ >= -Values.TrackLengthForward && endZ <= Values.TrackLengthBackward);
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            mpb = new MaterialPropertyBlock();
            meshRenderer.sortingLayerName = "Arc";
            meshRenderer.sortingOrder = 1;
        }
    }
}