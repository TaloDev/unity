namespace TaloGameServices
{
    [System.Serializable]
    public class ChannelsMembersResponse
    {
        public PlayerAlias[] members;
        public int count;
        public int itemsPerPage;
        public bool isLastPage;
    }
}
