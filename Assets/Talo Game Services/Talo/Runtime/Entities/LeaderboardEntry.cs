using System;
using UnityEngine;

namespace TaloGameServices
{
    public enum LeaderboardSortMode
    {
        DESC,
        ASC
    }

    [Serializable]
    public class LeaderboardEntry: EntityWithProps
    {
        public int id;
        public int position;
        public float score;
        public PlayerAlias playerAlias;
        public string createdAt;
        public string updatedAt;
        public string deletedAt;
        public string leaderboardName;
        public string leaderboardInternalName;
        [SerializeField] internal string leaderboardSortMode;
        
        public LeaderboardSortMode LeaderboardSortMode
        {
            get
            {
                return leaderboardSortMode.ToLower() == "asc"
                    ? LeaderboardSortMode.ASC
                    : LeaderboardSortMode.DESC;
            }
        }

        public override string ToString()
        {
            return $"#{position}: {score}";
        }
    }
}
