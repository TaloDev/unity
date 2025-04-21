using System;

namespace TaloGameServices
{
    public class RequestException : Exception
    {
        public long responseCode;

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
    }
}
