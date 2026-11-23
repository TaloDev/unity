using System;

namespace TaloGameServices
{
    [Serializable]
    public class PlayerRelationshipsBroadcastRequest
    {
        public int[] aliasIds;

        public string message;
    }
}
