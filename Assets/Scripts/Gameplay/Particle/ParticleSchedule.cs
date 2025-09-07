namespace ArcCreate.Gameplay.Particle
{
    public class ParticleSchedule
    {
        public readonly Particle Particle;
        public readonly float ExpireAt;
        public bool IsExpired = false;

        public ParticleSchedule(Particle _particle, float _expireAt)
        {
            Particle = _particle;
            ExpireAt = _expireAt;
        }
    }
}