namespace TaloGameServices
{
    [System.Serializable]
    public class StatsHistoryResponse
    {
        public PlayerStatSnapshot[] history;
        public int count;
        public int itemsPerPage;
        public bool isLastPage;
    }
}
