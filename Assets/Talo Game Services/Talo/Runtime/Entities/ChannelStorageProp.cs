using System;

namespace TaloGameServices
{
    [Serializable]
    public class ChannelStorageProp
    {
        public string key;
        public string value;
        public PlayerAlias createdBy;
        public PlayerAlias lastUpdatedBy;
        public string createdAt;
        public string updatedAt;
    }
}
