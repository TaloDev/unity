using UnityEngine;

namespace TaloGameServices
{
    [System.Serializable]
    public class SocketResponse
    {
        private class SocketMessage<T>
        {
            public string res;
            public T data;
        }

        public string message;

        public SocketResponse(string message) 
        {
            this.message = message;
        }

        public string GetResponseType()
        {
            var message = JsonUtility.FromJson<SocketMessage<object>>(this.message);
            return message.res;
        }

        public T GetData<T>()
        {
            return JsonUtility.FromJson<SocketMessage<T>>(message).data;
        }
    }
}
