using System;

namespace TaloGameServices
{
    [Serializable]
    public class PlayerRelationshipsSubscribeToRequest
    {
        public int aliasId;
        public string relationshipType;
    }
}
