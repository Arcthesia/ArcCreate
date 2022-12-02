using UnityEngine;

namespace ArcCreate.Gameplay.Particle
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Particle : MonoBehaviour
    {
        private ParticleSystem ps;
        private ParticleSystemRenderer render;
        private Transform cachedTransform;

        public Transform Transform => cachedTransform;

        public void Play() => ps.Play();

        public void Stop()
        {
            ps.Stop();
            ps.Clear();
        }

        public void ApplyMaterial(Material material)
        {
            render.material = material;
        }

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            render = GetComponent<ParticleSystemRenderer>();
            cachedTransform = transform;
        }
    }
}