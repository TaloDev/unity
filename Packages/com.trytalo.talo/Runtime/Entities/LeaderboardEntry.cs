using System;

namespace TaloGameServices
{
    [Serializable]
    public class LeaderboardEntry
    {
        public int id;
        public int position;
        public float score;
        public PlayerAlias playerAlias;
        public string updatedAt;

        public override string ToString()
        {
            return $"#{position}: {score}";
        }
    }
}
