namespace TaloGameServices
{
    [System.Serializable]
    public class Event
    {
        public string name;
        public int aliasId;
        public Prop[] props;
        public long timestamp;
    }
}
