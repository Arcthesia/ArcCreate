using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using UltraLiteDB;

namespace ArcCreate.Storage.Data
{
    public class PlayHistory
    {
        public const int MaxRecentPlaysCount = 10;
        public const int MaxTopScorePlaysCount = 10;

        private static Dictionary<(string levelId, string chartPath), PlayHistory> cache;

        [BsonId] public int Id { get; set; }

        public string LevelIdentifier { get; set; }

        public string ChartPath { get; set; }

        public List<PlayResult> RecentPlays { get; set; } = new List<PlayResult>();

        public List<PlayResult> TopScorePlays { get; set; } = new List<PlayResult>();

        public PlayResult BestScorePlay { get; set; }

        public PlayResult BestResultPlay { get; set; }

        [BsonIgnore] public PlayResult BestScorePlayOrDefault => BestScorePlay ?? new PlayResult();

        [BsonIgnore] public PlayResult BestResultPlayOrDefault => BestResultPlay ?? new PlayResult();

        public int PlayCount { get; set; }

        public static PlayHistory GetHistoryForChart(string levelId, string chartPath)
        {
            if (cache == null)
            {
                cache = new Dictionary<(string levelId, string chartPath), PlayHistory>();

                var collecion = Database.Current.GetCollection<PlayHistory>();
                List<PlayHistory> histories = collecion.FindAll().ToList();
                foreach (var history in histories)
                {
                    if (!cache.ContainsKey((history.LevelIdentifier, history.ChartPath)))
                    {
                        cache.Add((history.LevelIdentifier, history.ChartPath), history);
                    }
                }
            }

            cache.TryGetValue((levelId, chartPath), out PlayHistory alreadyExist);
            return alreadyExist ?? new PlayHistory()
            {
                LevelIdentifier = levelId,
                ChartPath = chartPath,
            };
        }

        public void AddPlay(PlayResult play)
        {
            RecentPlays.Add(play);
            RecentPlays.Sort((a, b) => -a.DateTime.CompareTo(b.DateTime));

            TopScorePlays.Add(play);
            TopScorePlays.Sort((a, b) => -a.Score.CompareTo(b.Score));

            for (int i = RecentPlays.Count - 1; i >= MaxRecentPlaysCount; i--)
            {
                RecentPlays.RemoveAt(i);
            }

            for (int i = TopScorePlays.Count - 1; i >= MaxTopScorePlaysCount; i--)
            {
                TopScorePlays.RemoveAt(i);
            }

            if (play.Score > BestScorePlayOrDefault.Score)
            {
                BestScorePlay = play;
            }

            if ((int)play.ClearResult > (int)BestResultPlayOrDefault.ClearResult)
            {
                BestResultPlay = play;
            }

            PlayCount += 1;
        }

        public void Delete()
        {
            Database.Current.GetCollection<PlayHistory>().Delete(Id);

            var key = (LevelIdentifier, ChartPath);
            cache.Remove(key);
        }

        public void Save()
        {
            var key = (LevelIdentifier, ChartPath);
            Database.Current.GetCollection<PlayHistory>().Upsert(this);
            if (cache.ContainsKey(key))
            {
                cache[key] = this;
            }
            else
            {
                cache.Add(key, this);
            }
        }
    }
}