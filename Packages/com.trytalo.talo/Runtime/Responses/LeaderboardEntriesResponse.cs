namespace TaloGameServices
{
    [System.Serializable]
    public class LeaderboardEntriesResponse
    {
        public LeaderboardEntry[] entries;
        public int count;
        public bool isLastPage;
    }
}
