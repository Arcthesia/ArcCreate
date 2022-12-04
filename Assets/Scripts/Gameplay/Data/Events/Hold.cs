namespace ArcCreate.Gameplay.Data
{
    public class Hold : LongNote, ILongNote<HoldBehaviour>
    {
        public int Lane { get; set; }

        public bool IsAssignedInstance => throw new System.NotImplementedException();

        public override ArcEvent Clone()
        {
            return new Hold()
            {
                Timing = Timing,
                EndTiming = EndTiming,
                TimingGroup = TimingGroup,
                Lane = Lane,
            };
        }

        public override void Assign(ArcEvent newValues)
        {
            base.Assign(newValues);
            Hold e = newValues as Hold;
            Lane = e.Lane;
        }

        public void AssignInstance(HoldBehaviour instance)
        {
            throw new System.NotImplementedException();
        }

        public HoldBehaviour RevokeInstance()
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

        public int CompareTo(INote<HoldBehaviour> other)
        {
            throw new System.NotImplementedException();
        }
    }
}
