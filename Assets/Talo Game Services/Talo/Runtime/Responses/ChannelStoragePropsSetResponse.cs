using System;

namespace TaloGameServices
{
    [Serializable]
    public class ChannelStoragePropsSetResponse
    {
        public Channel channel;
        public RejectedProp[] failedProps;
    }
}
