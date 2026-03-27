namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAuthMigrateAccountRequest
    {
        public string currentPassword;
        public string service;
        public string identifier;
    }
}
