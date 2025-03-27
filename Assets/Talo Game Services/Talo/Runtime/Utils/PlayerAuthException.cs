using System;

namespace TaloGameServices
{
    public enum PlayerAuthErrorCode {
        API_ERROR,
        INVALID_CREDENTIALS,
        VERIFICATION_ALIAS_NOT_FOUND,
        VERIFICATION_CODE_INVALID,
        IDENTIFIER_TAKEN,
        MISSING_SESSION,
        INVALID_SESSION,
        NEW_PASSWORD_MATCHES_CURRENT_PASSWORD,
        NEW_EMAIL_MATCHES_CURRENT_EMAIL,
        PASSWORD_RESET_CODE_INVALID,
        VERIFICATION_EMAIL_REQUIRED,
        INVALID_EMAIL
    }

    public class PlayerAuthException : Exception
    {
        public PlayerAuthErrorCode ErrorCode => GetErrorCode();

        public PlayerAuthException()
        {
        }

        public PlayerAuthException(string errorCode)
            : base(errorCode)
        {
        }

        public PlayerAuthException(string errorCode, Exception inner)
            : base(errorCode, inner)
        {
        }

        private PlayerAuthErrorCode GetErrorCode()
        {
            var errorCode = string.IsNullOrEmpty(Message) ? "API_ERROR" : Message;
            return (PlayerAuthErrorCode)Enum.Parse(typeof(PlayerAuthErrorCode), errorCode);
        }
    }
}
