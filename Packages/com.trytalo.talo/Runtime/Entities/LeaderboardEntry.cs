using System;

namespace TaloGameServices
{
    [Serializable]
    public class LeaderboardEntry: EntityWithProps
    {
        public int id;
        public int position;
        public float score;
        public PlayerAlias playerAlias;
        public string updatedAt;
        public string deletedAt;

        public override string ToString()
        {
            return $"#{position}: {score}";
        }
    }
}
