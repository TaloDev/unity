namespace TaloGameServices
{
    [System.Serializable]
    public class LeaderboardEntriesResponse
    {
        public LeaderboardEntry[] entries = new LeaderboardEntry[0];
        public int count;
        public bool isLastPage = true;
    }
}
