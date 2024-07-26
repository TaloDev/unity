namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAuthToggleVerificationRequest
    {
        public string currentPassword;
        public bool verificationEnabled;
        public string email;
    }
}
