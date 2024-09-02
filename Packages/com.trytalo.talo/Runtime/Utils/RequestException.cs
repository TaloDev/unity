using System;

namespace TaloGameServices
{
    public class RequestException : Exception
    {
        public RequestException()
        {
        }

        public RequestException(long responseCode)
            : base(responseCode.ToString())
        {
        }

        public RequestException(long responseCode, Exception inner)
            : base($"{responseCode}: {inner.Message}", inner)
        {
        }
    }
}
