namespace TaloGameServices
{
    [System.Serializable]
    public class ChannelsIndexResponse
    {
        public Channel[] channels;
        public int count;
        public int itemsPerPage;
        public bool isLastPage;
    }
}
