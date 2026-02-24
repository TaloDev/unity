namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAuthChangeIdentifierRequest
    {
        public string currentPassword;
        public string newIdentifier;
    }
}
