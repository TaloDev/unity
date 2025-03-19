using System;

namespace TaloGameServices
{
    public enum SocketErrorCode {
        API_ERROR,
        INVALID_MESSAGE,
        INVALID_MESSAGE_DATA,
        NO_PLAYER_FOUND,
        UNHANDLED_REQUEST,
        ROUTING_ERROR,
        LISTENER_ERROR,
        INVALID_SOCKET_TOKEN,
        INVALID_SESSION_TOKEN,
        MISSING_ACCESS_KEY_SCOPES,
        RATE_LIMIT_EXCEEDED
    }

    public class SocketException : Exception
    {
        private SocketError errorData;

        public string Req => errorData?.req ?? "unknown";
        public SocketErrorCode ErrorCode => GetErrorCode();
        public string Cause => errorData?.cause ?? "";

        public SocketException()
        {
        }

        public SocketException(SocketError errorData)
            : base(errorData.message)
        {
            this.errorData = errorData;
        }

        public SocketException(SocketError errorData, Exception inner)
            : base(errorData.message, inner)
        {
            this.errorData = errorData;
        }

        private SocketErrorCode GetErrorCode()
        {
            var errorCode = string.IsNullOrEmpty(errorData?.errorCode) ? "API_ERROR" : errorData.errorCode;
            return (SocketErrorCode)Enum.Parse(typeof(SocketErrorCode), errorCode);
        }
    }
}
