using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public class InstancedRendererPool
    {
        private readonly Material material;
        private readonly Mesh mesh;
        private readonly bool useProperties;
        private readonly List<InstancedRenderer> renderers = new List<InstancedRenderer>();
        private readonly List<Material> materials = new List<Material>();

        private int index = 0;

        public InstancedRendererPool(Material material, Mesh mesh, bool useProperties)
        {
            this.material = material;
            this.mesh = mesh;
            this.useProperties = useProperties;
            CreateNewRenderer();
        }

        public void RegisterInstance(Matrix4x4 matrix, Color color, Vector4 property = default)
        {
            bool accepted = renderers[index].RegisterInstance(matrix, color, property);
            if (!accepted)
            {
                index += 1;
                if (index >= renderers.Count)
                {
                    CreateNewRenderer();
                }

                RegisterInstance(matrix, property);
            }
        }

        public void Draw(Camera camera, LayerMask layerMask)
        {
            for (int i = 0; i <= index; i++)
            {
                InstancedRenderer renderer = renderers[i];
                renderer.Draw(camera, layerMask);
            }

            index = 0;
        }

        public void Dispose()
        {
            for (int i = 0; i < materials.Count; i++)
            {
                Material mat = materials[i];
                Object.Destroy(mat);
            }

            materials.Clear();
        }

        private void CreateNewRenderer()
        {
            Material newMat = Object.Instantiate(material);
            materials.Add(newMat);
            renderers.Add(new InstancedRenderer(newMat, mesh, useProperties));
        }
    }
}