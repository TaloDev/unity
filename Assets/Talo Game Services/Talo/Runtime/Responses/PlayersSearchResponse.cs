namespace TaloGameServices
{
    [System.Serializable]
    public class PlayersSearchResponse
    {
        public Player[] players;
        public int count;
        public int itemsPerPage;
        public bool isLastPage;
    }
}
