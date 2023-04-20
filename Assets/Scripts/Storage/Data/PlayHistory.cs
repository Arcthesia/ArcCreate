using System.Collections.Generic;
using UltraLiteDB;

namespace ArcCreate.Storage.Data
{
    public class PlayHistory
    {
        public const int MaxRecentPlaysCount = 10;
        public const int MaxTopScorePlaysCount = 10;

        [BsonId] public int Id { get; set; }

        public string LevelIdentifier { get; set; }

        public string ChartPath { get; set; }

        public List<PlayResult> RecentPlays { get; set; }

        public List<PlayResult> TopScorePlays { get; set; }

        public PlayResult BestScorePlay { get; set; }

        public PlayResult BestResultPlay { get; set; }

        [BsonId] public PlayResult BestPlay => TopScorePlays[0];

        public void AddPlay(PlayResult play)
        {
            RecentPlays.Add(play);
            RecentPlays.Sort((a, b) => a.DateTime.CompareTo(b.DateTime));

            TopScorePlays.Add(play);
            TopScorePlays.Sort((a, b) => a.Score.CompareTo(b.Score));

            for (int i = RecentPlays.Count - 1; i >= MaxRecentPlaysCount; i--)
            {
                RecentPlays.RemoveAt(i);
            }

            for (int i = TopScorePlays.Count - 1; i >= MaxTopScorePlaysCount; i--)
            {
                TopScorePlays.RemoveAt(i);
            }

            if (play.Score > BestScorePlay.Score)
            {
                BestScorePlay = play;
            }

            if ((int)play.ClearResult > (int)BestResultPlay.ClearResult)
            {
                BestResultPlay = play;
            }
        }

        public void Delete()
        {
            Database.Current.GetCollection<PlayHistory>().Delete(Id);
        }
    }
}