namespace ArcCreate.Gameplay.Data
{
    public class ArcTap : Note, INote<ArcTapBehaviour>
    {
        public Arc Arc { get; set; }

        public float WorldX => Arc.WorldXAt(Timing);

        public float WorldY => Arc.WorldYAt(Timing);

        public string Sfx => Arc.Sfx;

        public bool IsAssignedInstance => throw new System.NotImplementedException();

        public override ArcEvent Clone()
        {
            return new ArcTap()
            {
                Timing = Timing,
                Arc = Arc,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            ArcTap e = newValues as ArcTap;
            Arc = e.Arc;
        }

        public void AssignInstance(ArcTapBehaviour instance)
        {
            throw new System.NotImplementedException();
        }

        public ArcTapBehaviour RevokeInstance()
        {
            throw new System.NotImplementedException();
        }

        public void ResetJudge()
        {
            throw new System.NotImplementedException();
        }

        public void Rebuild()
        {
            throw new System.NotImplementedException();
        }

        public void ReloadSkin()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateJudgement(int timing, GroupProperties groupProperties)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateInstance(int timing, double floorPosition, GroupProperties groupProperties)
        {
            throw new System.NotImplementedException();
        }

        public int CompareTo(INote<ArcTapBehaviour> other)
        {
            throw new System.NotImplementedException();
        }
    }
}
