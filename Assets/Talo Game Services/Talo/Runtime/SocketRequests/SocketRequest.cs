namespace TaloGameServices
{
    [System.Serializable]
    public class SocketRequest<T>
    {
        public string req;

        public T data;

        public SocketRequest(string requestType, T requestData) 
        {
            req = requestType;
            data = requestData;
        }
    }
}
