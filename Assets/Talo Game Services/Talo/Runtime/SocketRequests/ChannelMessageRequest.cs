namespace TaloGameServices
{
    [System.Serializable]
    public class ChannelMessageRequest
    {
        [System.Serializable]
        public class ChannelStub
        {
            public int id;
        }

        public ChannelStub channel;
        public string message;
    }
}
