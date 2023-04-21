using System.Collections.Generic;
using ArcCreate.Data;
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

        public List<PlayResult> RecentPlays { get; set; } = new List<PlayResult>();

        public List<PlayResult> TopScorePlays { get; set; } = new List<PlayResult>();

        public PlayResult BestScorePlay { get; set; }

        public PlayResult BestResultPlay { get; set; }

        public int PlayCount { get; set; }

        [BsonId] public PlayResult BestPlay => TopScorePlays[0];

        public static PlayHistory GetHistoryForChart(string levelId, string chartPath)
        {
            var collecion = Database.Current.GetCollection<PlayHistory>();
            PlayHistory alreadyExist = collecion.FindOne(Query.And(Query.EQ("LevelIdentifier", levelId), Query.EQ("ChartPath", chartPath)));
            return alreadyExist ?? new PlayHistory()
            {
                LevelIdentifier = levelId,
                ChartPath = chartPath,
            };
        }

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

            PlayCount += 1;
        }

        public void Delete()
        {
            Database.Current.GetCollection<PlayHistory>().Delete(Id);
        }

        public void Save()
        {
            Database.Current.GetCollection<PlayHistory>().Upsert(this);
        }
    }
}