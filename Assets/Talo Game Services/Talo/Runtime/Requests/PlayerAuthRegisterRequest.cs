namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAuthRegisterRequest
    {
        public string identifier;
        public string password;
        public string email;
        public bool verificationEnabled;
    }
}
