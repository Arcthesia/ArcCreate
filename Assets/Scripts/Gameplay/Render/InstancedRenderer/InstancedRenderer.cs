using UnityEngine;

public class InstancedRenderer<T>
    where T : struct
{
    public const int Population = 511;

    private Material material;
    private readonly Mesh mesh;
    private readonly int propertyShaderId;
    private readonly Matrix4x4[] matrices = new Matrix4x4[Population];
    private readonly T[] properties = new T[Population];
    private readonly ComputeBuffer propertyBuffer;

    private int count = 0;

    public InstancedRenderer(Material material, Mesh mesh, int propertyShaderId, int propertySize)
    {
        this.material = material;
        this.mesh = mesh;
        this.propertyShaderId = propertyShaderId;

        propertyBuffer = new ComputeBuffer(Population, propertySize);
        material.SetBuffer(propertyShaderId, propertyBuffer);
    }

    public void SetMaterial(Material material)
    {
        this.material = material;
        material.SetBuffer(propertyShaderId, propertyBuffer);
    }

    public bool RegisterInstance(Matrix4x4 matrix, T property)
    {
        if (count >= Population)
        {
            return false;
        }

        matrices[count] = matrix;
        properties[count] = property;
        count++;
        return true;
    }

    public void Draw(Camera camera, LayerMask layerMask)
    {
        if (count > 0)
        {
            propertyBuffer.SetData(properties);

            Graphics.DrawMeshInstanced(
                mesh: mesh,
                submeshIndex: 0,
                material: material,
                matrices: matrices,
                count: count,
                properties: null,
                castShadows: UnityEngine.Rendering.ShadowCastingMode.Off,
                receiveShadows: false,
                layer: layerMask,
                camera: camera);
        }

        count = 0;
    }

    public void Dispose()
    {
        propertyBuffer.Release();
    }
}