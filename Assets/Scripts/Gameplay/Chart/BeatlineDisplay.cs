using System.Collections.Generic;

namespace ArcCreate.Gameplay.Chart
{
    public class BeatlineDisplay
    {
        private CachedBisect<Beatline, double> floorPositionSearch = new CachedBisect<Beatline, double>(new List<Beatline>(), x => x.FloorPosition);
        private readonly List<Beatline> previousBeatlinesInRange = new List<Beatline>(32);

        private readonly IBeatlineGenerator generator;
        private readonly Pool<BeatlineBehaviour> beatlinePool;

        public BeatlineDisplay(IBeatlineGenerator generator, Pool<BeatlineBehaviour> beatlinePool)
        {
            this.generator = generator;
            this.beatlinePool = beatlinePool;
        }

        public List<Beatline> LoadFromTimingGroup(int tgNum, int audioLength)
        {
            beatlinePool.ReturnAll();
            previousBeatlinesInRange.Clear();
            TimingGroup tg = Services.Chart.GetTimingGroup(tgNum);
            List<Beatline> beatlines = new List<Beatline>(generator.Generate(tg, audioLength));
            floorPositionSearch = new CachedBisect<Beatline, double>(beatlines, x => x.FloorPosition);

            return beatlines;
        }

        public void UpdateBeatlines(double floorPosition)
        {
            if (floorPositionSearch.List.Count == 0)
            {
                return;
            }

            double fpDistForward = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLengthForward));
            double fpDistBackward = System.Math.Abs(ArcFormula.ZToFloorPosition(Values.TrackLengthBackward));
            double renderFrom = floorPosition - fpDistBackward;
            double renderTo = floorPosition + fpDistForward;

            int renderIndex = floorPositionSearch.Bisect(renderFrom);

            // Disable old notes
            for (int i = 0; i < previousBeatlinesInRange.Count; i++)
            {
                Beatline beatline = previousBeatlinesInRange[i];
                if (beatline.FloorPosition < renderFrom || beatline.FloorPosition > renderTo)
                {
                    beatlinePool.Return(beatline.RevokeInstance());
                }
            }

            previousBeatlinesInRange.Clear();

            // Update notes
            while (renderIndex < floorPositionSearch.List.Count)
            {
                Beatline beatline = floorPositionSearch.List[renderIndex];
                if (beatline.FloorPosition > renderTo)
                {
                    break;
                }

                if (!beatline.IsAssignedInstance)
                {
                    beatline.AssignInstance(beatlinePool.Get());
                }

                beatline.UpdateInstance(floorPosition);
                renderIndex++;
                previousBeatlinesInRange.Add(beatline);
            }
        }
    }
}