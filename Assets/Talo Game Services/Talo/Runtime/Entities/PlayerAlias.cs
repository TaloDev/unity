namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAlias
    {
        public int id;
        public string service, identifier;
        public Player player;
        public string lastSeenAt, createdAt, updatedAt;
    }
}
