namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAlias
    {
        public int id;
        public string service, identifier;
        public Player player;
        public string lastSeenAt, createdAt, updatedAt;

        public bool MatchesIdentifyRequest(string service, string identifier)
        {
            return this.service == service && this.identifier == identifier;
        }
    }
}
