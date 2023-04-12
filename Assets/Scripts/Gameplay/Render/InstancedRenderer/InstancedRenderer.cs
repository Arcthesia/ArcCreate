using UnityEngine;

public class InstancedRenderer
{
    public const int Population = 127;
    public static readonly int ColorShaderId = Shader.PropertyToID("_Color");
    public static readonly int PropertyShaderId = Shader.PropertyToID("_Properties");

    private readonly Material material;
    private readonly Mesh mesh;
    private readonly Matrix4x4[] matrices = new Matrix4x4[Population];
    private readonly Vector4[] colors = new Vector4[Population];
    private readonly Vector4[] properties = new Vector4[Population];
    private readonly bool useProperties;
    private readonly MaterialPropertyBlock mpb;

    private int count = 0;

    public InstancedRenderer(Material material, Mesh mesh, bool useProperties)
    {
        this.material = material;
        this.mesh = mesh;
        this.useProperties = useProperties;
        mpb = new MaterialPropertyBlock();
    }

    public bool RegisterInstance(Matrix4x4 matrix, Color color, Vector4 property = default)
    {
        if (count >= Population)
        {
            return false;
        }

        matrices[count] = matrix;
        colors[count] = color;
        properties[count] = property;
        count++;
        return true;
    }

    public void Draw(Camera camera, LayerMask layerMask)
    {
        if (count > 0)
        {
            mpb.SetVectorArray(ColorShaderId, colors);
            if (useProperties)
            {
                mpb.SetVectorArray(PropertyShaderId, properties);
            }

            Graphics.DrawMeshInstanced(
                mesh: mesh,
                submeshIndex: 0,
                material: material,
                matrices: matrices,
                count: count,
                properties: mpb,
                castShadows: UnityEngine.Rendering.ShadowCastingMode.Off,
                receiveShadows: false,
                layer: layerMask,
                camera: camera);
        }

        count = 0;
    }
}