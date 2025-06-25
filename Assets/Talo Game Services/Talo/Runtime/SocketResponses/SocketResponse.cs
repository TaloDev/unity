using System;
using UnityEngine;

namespace TaloGameServices
{
    [Serializable]
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
            var json = JsonUtility.FromJson<SocketMessage<object>>(message);
            return json.res;
        }

        public T GetData<T>()
        {
            return JsonUtility.FromJson<SocketMessage<T>>(message).data;
        }

        public object GetJsonData()
        {
            var json = message.Substring(1, message.Length - 2) // remove the curly braces
                .Replace("\"res\":\"" + GetResponseType() + "\",", "") // remove the response type
                .Replace("\"data\":", ""); // remove the data key, only keep the value
            return json;
        }
    }
}
