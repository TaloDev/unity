namespace TaloGameServices
{
    [System.Serializable]
    public class ChannelOwnershipTransferredResponse
    {
        public Channel channel;
        public PlayerAlias newOwner;
    }
}
