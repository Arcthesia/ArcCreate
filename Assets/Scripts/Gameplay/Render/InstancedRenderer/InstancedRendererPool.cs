using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay.Render
{
    public class InstancedRendererPool<T>
        where T : struct
    {
        private Material material;
        private readonly Mesh mesh;
        private readonly int propertyShaderId;
        private readonly int propertySize;

        private readonly List<InstancedRenderer<T>> renderers = new List<InstancedRenderer<T>>();
        private readonly List<Material> materials = new List<Material>();

        private int index = 0;

        public InstancedRendererPool(Material material, Mesh mesh, int propertyShaderId, int propertySize)
        {
            this.material = material;
            this.mesh = mesh;
            this.propertyShaderId = propertyShaderId;
            this.propertySize = propertySize;
            CreateNewRenderer();
        }

        public void SetMaterial(Material material)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                Object.Destroy(materials[i]);
                materials[i] = Object.Instantiate(material);
            }

            this.material = material;

            for (int i = 0; i < renderers.Count; i++)
            {
                InstancedRenderer<T> renderer = renderers[i];
                renderer.SetMaterial(material);
            }
        }

        public void RegisterInstance(Matrix4x4 matrix, T property)
        {
            bool accepted = renderers[index].RegisterInstance(matrix, property);
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
                InstancedRenderer<T> renderer = renderers[i];
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

            for (int i = 0; i < renderers.Count; i++)
            {
                InstancedRenderer<T> renderer = renderers[i];
                renderer.Dispose();
            }
        }

        private void CreateNewRenderer()
        {
            Material newMat = Object.Instantiate(material);
            materials.Add(newMat);
            renderers.Add(new InstancedRenderer<T>(newMat, mesh, propertyShaderId, propertySize));
        }
    }
}