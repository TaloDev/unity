namespace TaloGameServices
{
    [System.Serializable]
    public class PlayersGameCenterIdentifier
    {
        public string publicKeyUrl;
        public string signature; // base64 encoded
        public string salt; // base64 encoded
        public ulong timestamp;
        public string playerId;
        public string bundleId;
    }
}
