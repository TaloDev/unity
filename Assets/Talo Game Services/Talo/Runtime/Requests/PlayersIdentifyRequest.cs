namespace TaloGameServices
{
    [System.Serializable]
    public class PlayersIdentifyRequest
    {
        public string service, identifier;

        public PlayersIdentifyRequest(string service, string identifier)
        {
            this.service = service;
            this.identifier = identifier;
        }
    }
}
