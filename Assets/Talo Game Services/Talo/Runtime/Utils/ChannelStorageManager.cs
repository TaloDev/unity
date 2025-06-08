using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaloGameServices
{
    public class ChannelStorageManager
    {
        private Dictionary<string, ChannelStorageProp> _currentProps = new();

        public void OnPropsUpdated(Channel channel, ChannelStorageProp[] upsertedProps, ChannelStorageProp[] deletedProps)
        {
            foreach (var prop in upsertedProps)
            {
                UpsertProp(channel.id, prop);
            }

            foreach (var prop in deletedProps)
            {
                DeleteProp(channel.id, prop.key);
            }
        }

        public async Task<ChannelStorageProp> GetProp(int channelId, string key)
        {
            var cacheKey = $"{channelId}:{key}";
            if (_currentProps.TryGetValue(cacheKey, out var cachedProp))
            {
                return cachedProp;
            }

            return await Talo.Channels.GetStorageProp(channelId, key, true);
        }

        public void UpsertProp(int channelId, ChannelStorageProp prop)
        {
            var key = $"{channelId}:{prop.key}";
            _currentProps[key] = prop;
        }

        public void DeleteProp(int channelId, string propKey)
        {
            var key = $"{channelId}:{propKey}";
            _currentProps.Remove(key);
        }
    }
}
