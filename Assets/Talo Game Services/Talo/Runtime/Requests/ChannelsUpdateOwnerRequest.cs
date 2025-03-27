namespace TaloGameServices
{
    [System.Serializable]
    public class ChannelsUpdateOwnerRequest
    {
        public string name;
        public int newOwnerAliasId;
        public Prop[] props;
    }
}
