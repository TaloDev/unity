using System;

namespace TaloGameServices
{
    [Serializable]
    public class ChannelStorageUpdatedResponse
    {
        public Channel channel;
        public ChannelStorageProp[] upsertedProps;
        public ChannelStorageProp[] deletedProps;
    }
}
