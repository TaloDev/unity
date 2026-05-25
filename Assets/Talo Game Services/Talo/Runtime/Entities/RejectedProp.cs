using System;
using UnityEngine;

namespace TaloGameServices
{
    public enum PropRejectionReason
    {
        UNKNOWN_ERROR,
        PROP_KEY_TOO_LONG,
        PROP_VALUE_TOO_LONG,
        PROP_ARRAY_TOO_LONG,
        PROP_CONTAINS_PROFANITY,
        PROP_KEY_RESERVED
    }

    [Serializable]
    public class RejectedProp
    {
        public string key;
        public string error;
        public string message;

        public PropRejectionReason GetReason()
        {
            if (Enum.TryParse(error, out PropRejectionReason reason))
            {
                return reason;
            }
            return PropRejectionReason.UNKNOWN_ERROR;
        }

        public static RejectedProp[] FromJson(string json)
        {
            var wrapper = JsonUtility.FromJson<RejectedPropWrapper>(json);
            return wrapper?.rejectedProps ?? Array.Empty<RejectedProp>();
        }

        public static void TryEmit(string json, Action<RejectedProp[]> onRejected)
        {
            var rejectedProps = FromJson(json);
            if (rejectedProps.Length > 0)
            {
                onRejected?.Invoke(rejectedProps);
            }
        }
    }

    [Serializable]
    public class RejectedPropWrapper
    {
        public RejectedProp[] rejectedProps;
    }
}
