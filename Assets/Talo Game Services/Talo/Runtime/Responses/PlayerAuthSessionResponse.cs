namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAuthSessionResponse
    {
        public PlayerAlias alias;
        public string sessionToken;
        public string socketToken;
    }
}
