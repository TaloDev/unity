using System;

namespace TaloGameServices
{
    [Serializable]
    public class ChannelStoragePropError
    {
        public string key;
        public string error;
        public string message;
    }

    [Serializable]
    public class ChannelStoragePropsSetResponse
    {
        public Channel channel;
        public ChannelStoragePropError[] failedProps;
    }
}
