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
        private readonly SocketError errorData;

        public string Req => errorData?.req ?? "unknown";
        public SocketErrorCode ErrorCode { get; }
        public string Cause => errorData?.cause ?? "";

        public SocketException()
        {
            ErrorCode = SocketErrorCode.API_ERROR;
        }

        public SocketException(SocketError errorData)
            : base(errorData.message)
        {
            this.errorData = errorData;
            ErrorCode = ParseErrorCode(errorData?.errorCode);
        }

        public SocketException(SocketError errorData, Exception inner)
            : base(errorData.message, inner)
        {
            this.errorData = errorData;
            ErrorCode = ParseErrorCode(errorData?.errorCode);
        }

        private static SocketErrorCode ParseErrorCode(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                return SocketErrorCode.API_ERROR;
            }
        
            return Enum.TryParse(errorCode, out SocketErrorCode code)
                ? code
                : SocketErrorCode.API_ERROR;
        }
    }
}
