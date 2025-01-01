namespace TaloGameServices
{
    [System.Serializable]
    public class ChannelsUpdateRequest
    {
        public string name;
        public int newOwnerAliasId;
        public Prop[] props;
    }
}
