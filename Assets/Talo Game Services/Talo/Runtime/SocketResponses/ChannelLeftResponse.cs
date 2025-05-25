namespace TaloGameServices
{
    [System.Serializable]
    public class ChannelLeftResponse
    {
        public Channel channel;
        public PlayerAlias playerAlias;
        public ChannelLeftResponseMetadata meta;
    }

    [System.Serializable]
    public class ChannelLeftResponseMetadata
    {
        public ChannelLeavingReason reason;
    }
}
