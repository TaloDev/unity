namespace TaloGameServices
{
    [System.Serializable]
    public class SocketError
    {
        public string req;
        public string message;
        public string errorCode;
        public string cause;

        public void Throw()
        {
            throw new SocketException(this);
        }
    }
}
