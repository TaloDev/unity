using System;

namespace TaloGameServices
{
    [Serializable]
    public class Channel: EntityWithProps
    {
        public int id;
        public string name;
        public PlayerAlias ownerAlias;
        public int totalMessages;
        public int memberCount;
        public string createdAt;
        public string updatedAt;
    }
}
