namespace TaloGameServices
{
    [System.Serializable]
    public class ChannelsCreateRequest
    {
        public string name;
        public bool autoCleanup;
        public Prop[] props;
        public bool @private;
    }
}
