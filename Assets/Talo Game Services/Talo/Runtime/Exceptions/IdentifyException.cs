using System;
using UnityEngine;

namespace TaloGameServices
{
    public enum IdentifyErrorCode
    {
        UNKNOWN_ERROR,
        IDENTIFIER_PROFANITY,
        IDENTIFIER_TAKEN
    }

    public class IdentifyException : Exception
    {
        public IdentifyErrorCode ErrorCode { get; }

        public IdentifyException(IdentifyErrorCode code = IdentifyErrorCode.UNKNOWN_ERROR)
            : base(code.ToString())
        {
            ErrorCode = code;
        }

        public IdentifyException(IdentifyErrorCode code, Exception inner)
            : base(code.ToString(), inner)
        {
            ErrorCode = code;
        }

        public static IdentifyException FromException(Exception ex)
        {
            if (ex is RequestException re && !string.IsNullOrEmpty(re.responseBody))
            {
                return FromResponse(re.responseBody);
            }

            return new IdentifyException();
        }

        private static IdentifyException FromResponse(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return new IdentifyException();
            }

            try
            {
                var parsed = JsonUtility.FromJson<ErrorResponse>(body);
                if (parsed != null && !string.IsNullOrEmpty(parsed.errorCode) &&
                    Enum.TryParse(parsed.errorCode, out IdentifyErrorCode code))
                {
                    return new IdentifyException(code);
                }
            }
            catch
            {
            }

            return new IdentifyException();
        }
    }
}
