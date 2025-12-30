using UnityEngine;

namespace ArcCreate.Gameplay.Particle
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Particle : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sprite;

        private ParticleSystem ps;
        private ParticleSystemRenderer render;
        private Transform cachedTransform;

        public Transform Transform => cachedTransform;

        public void Play()
        {
            ps.Play();
            if (sprite != null)
            {
                sprite.enabled = true;
            }
        }

        public void Emit(Vector3 position)
        {
            ParticleSystem.EmitParams emitParams = new ()
            {
                position = ps.transform.InverseTransformPoint(position),
                applyShapeToPosition = true,
            };
            ps.Emit(emitParams, 1);
        }

        public void Stop()
        {
            ps.Stop();
            ps.Clear();
            if (sprite != null)
            {
                sprite.enabled = false;
            }
        }

        public void ApplyMaterial(Material material)
        {
            render.material = material;
        }

        public void ApplyColor(Color color1, Color color2)
        {
            ParticleSystem.MainModule module = ps.main;
            module.startColor = new ParticleSystem.MinMaxGradient(color1, color2);
            if (sprite != null)
            {
                color2.a = 1;
                sprite.color = color2;
            }
        }

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            render = GetComponent<ParticleSystemRenderer>();
            cachedTransform = transform;
        }
    }
}