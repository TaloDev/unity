using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaloGameServices
{
    public class ChannelStorageManager
    {
        private readonly Dictionary<int, List<ChannelStorageProp>> _channelProps = new();

        private List<ChannelStorageProp> GetOrCreateList(int channelId)
        {
            if (!_channelProps.TryGetValue(channelId, out var list))
            {
                list = new List<ChannelStorageProp>();
                _channelProps[channelId] = list;
            }
            return list;
        }

        [Serializable]
        private class StringArrayWrapper { public string[] items; }

        private static string[] ParseJsonStringArray(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
            {
                return Array.Empty<string>();
            }

            var wrapped = UnityEngine.JsonUtility.FromJson<StringArrayWrapper>("{\"items\":" + json + "}");
            return wrapped?.items ?? Array.Empty<string>();
        }

        public void OnPropsUpdated(Channel channel, ChannelStorageProp[] upsertedProps, ChannelStorageProp[] deletedProps)
        {
            UpsertManyProps(channel.id, upsertedProps);

            foreach (var prop in deletedProps)
            {
                DeleteProp(channel.id, prop.key);
            }
        }

        public async Task<ChannelStorageProp> GetProp(int channelId, string key)
        {
            var list = GetOrCreateList(channelId);
            var cached = list.FirstOrDefault((p) => p.key == key);
            if (cached != null)
            {
                return cached;
            }

            return await Talo.Channels.GetStorageProp(channelId, key, true);
        }

        public void UpsertProp(int channelId, ChannelStorageProp prop, bool expand = false)
        {
            if (expand && prop.key.EndsWith("[]"))
            {
                var values = ParseJsonStringArray(prop.value);
                if (values.Length > 0)
                {
                    var expandedProps = values.Select((v) => new ChannelStorageProp
                    {
                        key = prop.key,
                        value = v,
                        createdBy = prop.createdBy,
                        lastUpdatedBy = prop.lastUpdatedBy,
                        createdAt = prop.createdAt,
                        updatedAt = prop.updatedAt
                    }).ToArray();

                    UpsertManyProps(channelId, expandedProps);
                    return;
                }
            }

            var list = GetOrCreateList(channelId);
            list.RemoveAll((p) => p.key == prop.key);
            list.Add(prop);
        }

        public void UpsertManyProps(int channelId, ChannelStorageProp[] props)
        {
            var list = GetOrCreateList(channelId);
            var keys = props.Select((p) => p.key).Distinct().ToHashSet();
            list.RemoveAll((p) => keys.Contains(p.key));
            list.AddRange(props);
        }

        public void DeleteProp(int channelId, string propKey)
        {
            GetOrCreateList(channelId).RemoveAll((p) => p.key == propKey);
        }

        internal ChannelStorageProp[] GetCachedProps(int channelId)
        {
            return GetOrCreateList(channelId).ToArray();
        }


        public async Task<ChannelStorageProp[]> ListProps(int channelId, string[] propKeys)
        {
            var list = GetOrCreateList(channelId);
            var results = new List<ChannelStorageProp>();
            var keysToFetch = new List<string>();

            foreach (var propKey in propKeys)
            {
                var cached = list.Where((p) => p.key == propKey).ToArray();
                if (cached.Length > 0)
                {
                    results.AddRange(cached);
                }
                else
                {
                    keysToFetch.Add(propKey);
                }
            }

            if (keysToFetch.Count > 0)
            {
                var fetchedProps = await Talo.Channels.ListStorageProps(channelId, keysToFetch.ToArray(), true);
                results.AddRange(fetchedProps);
            }

            return results.ToArray();
        }
    }
}
