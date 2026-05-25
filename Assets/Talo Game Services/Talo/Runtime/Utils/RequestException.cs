using System;

namespace TaloGameServices
{
    public class RequestException : Exception
    {
        public long responseCode;
        public string responseBody;

        public RequestException()
        {
        }

        public RequestException(long responseCode)
            : base(responseCode.ToString())
        {
            this.responseCode = responseCode;
        }

        public RequestException(long responseCode, Exception inner)
            : base($"{responseCode}: {inner.Message}", inner)
        {
            this.responseCode = responseCode;
        }

        public RequestException(long responseCode, Exception inner, string responseBody)
            : base($"{responseCode}: {inner.Message}", inner)
        {
            this.responseCode = responseCode;
            this.responseBody = responseBody;
        }

        public bool IsBadRequest()
        {
            return responseCode == 400 && !string.IsNullOrEmpty(responseBody);
        }
    }
}
