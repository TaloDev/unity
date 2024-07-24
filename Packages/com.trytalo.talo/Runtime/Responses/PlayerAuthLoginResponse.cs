namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAuthLoginResponse: PlayerAuthSessionResponse
    {
        public int aliasId;
        public bool verificationRequired;
    }
}
