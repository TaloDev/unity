namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerPresenceUpdatedResponse
    {
        public PlayerPresence presence;
        public PresenceMetadata meta;
    }

    [System.Serializable]
    public class PresenceMetadata
    {
        public bool onlineChanged;
        public bool customStatusChanged;
    }
}
