namespace TaloGameServices
{
    [System.Serializable]
    public class LeaderboardEntriesResponse
    {
        public LeaderboardEntry[] entries;
        public int count;
        public int itemsPerPage;
        public bool isLastPage;
    }
}
