using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class GetChannelsOptions
    {
        public int page = 0;
        public string propKey = "";
        public string propValue = "";

        public string ToQueryString()
        {
            var query = new Dictionary<string, string> { ["page"] = page.ToString() };
            if (!string.IsNullOrEmpty(propKey)) query["propKey"] = propKey;
            if (!string.IsNullOrEmpty(propValue)) query["propValue"] = propValue;

            return string.Join("&", query.Select((param) => $"{param.Key}={param.Value}"));
        }
    }

    public class GetSubscribedChannelsOptions
    {
        public string propKey = "";
        public string propValue = "";

        public string ToQueryString()
        {
            var query = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(propKey)) query["propKey"] = propKey;
            if (!string.IsNullOrEmpty(propValue)) query["propValue"] = propValue;

            return string.Join("&", query.Select((param) => $"{param.Key}={param.Value}"));
        }
    }

    public class CreateChannelOptions
    {
        public string name;
        public (string, string)[] props = Array.Empty<(string, string)>();
        public bool autoCleanup = false;
        public bool isPrivate = false;
        public bool temporaryMembership = false;
    }

    public enum ChannelLeavingReason
    {
        Default,
        TemporaryMembership
    }

    public class ChannelsAPI : BaseAPI
    {
        public event Action<Channel, PlayerAlias, string> OnMessageReceived;
        public event Action<Channel, PlayerAlias> OnChannelJoined;
        public event Action<Channel, PlayerAlias, ChannelLeavingReason> OnChannelLeft;
        public event Action<Channel, PlayerAlias> OnOwnershipTransferred;
        public event Action<Channel> OnChannelDeleted;
        public event Action<Channel, string[]> OnChannelUpdated;
        public event Action<Channel, ChannelStoragePropError[]> OnChannelStoragePropsFailedToSet;
        public event Action<Channel, ChannelStorageProp[], ChannelStorageProp[]> OnChannelStoragePropsUpdated;

        private ChannelStorageManager _storageManager = new ChannelStorageManager();

        public ChannelsAPI() : base("v1/game-channels")
        {
            Talo.Socket.OnMessageReceived += (response) =>
            {
                if (response.GetResponseType() == "v1.channels.message")
                {
                    var data = response.GetData<ChannelMessageResponse>();
                    OnMessageReceived?.Invoke(data.channel, data.playerAlias, data.message);
                }
                else if (response.GetResponseType() == "v1.channels.joined")
                {
                    var data = response.GetData<ChannelJoinedResponse>();
                    OnChannelJoined?.Invoke(data.channel, data.playerAlias);
                }
                else if (response.GetResponseType() == "v1.channels.left")
                {
                    var data = response.GetData<ChannelLeftResponse>();
                    OnChannelLeft?.Invoke(data.channel, data.playerAlias, data.meta.reason);
                }
                else if (response.GetResponseType() == "v1.channels.ownership-transferred")
                {
                    var data = response.GetData<ChannelOwnershipTransferredResponse>();
                    OnOwnershipTransferred?.Invoke(data.channel, data.newOwner);
                }
                else if (response.GetResponseType() == "v1.channels.deleted")
                {
                    var data = response.GetData<ChannelDeletedResponse>();
                    OnChannelDeleted?.Invoke(data.channel);
                }
                else if (response.GetResponseType() == "v1.channels.updated")
                {
                    var data = response.GetData<ChannelUpdatedResponse>();
                    OnChannelUpdated?.Invoke(data.channel, data.changedProperties);
                }
                else if (response.GetResponseType() == "v1.channels.storage.updated")
                {
                    var data = response.GetData<ChannelStorageUpdatedResponse>();
                    OnChannelStoragePropsUpdated?.Invoke(data.channel, data.upsertedProps, data.deletedProps);
                }
            };

            OnChannelStoragePropsUpdated += _storageManager.OnPropsUpdated;
        }

        public async Task<ChannelsIndexResponse> GetChannels(GetChannelsOptions options = null)
        {
            options ??= new GetChannelsOptions();

            var uri = new Uri($"{baseUrl}?{options.ToQueryString()}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<ChannelsIndexResponse>(json);
            return res;
        }

        [Obsolete("Use GetChannels(GetChannelsOptions options) instead.")]
        public async Task<ChannelsIndexResponse> GetChannels(int page)
        {
            return await GetChannels(new GetChannelsOptions { page = page });
        }

        public async Task<Channel[]> GetSubscribedChannels(GetSubscribedChannelsOptions options = null)
        {
            Talo.IdentityCheck();

            options ??= new GetSubscribedChannelsOptions();

            var uri = new Uri($"{baseUrl}/subscriptions?{options.ToQueryString()}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<ChannelsSubscriptionsResponse>(json);
            return res.channels;
        }

        private async Task<Channel> SendCreateChannelRequest(CreateChannelOptions options)
        {
            Talo.IdentityCheck();

            var props = options.props.Select((propTuple) => new Prop(propTuple)).ToArray();

            var uri = new Uri(baseUrl);
            var content = JsonUtility.ToJson(new ChannelsCreateRequest
            {
                name = options.name,
                autoCleanup = options.autoCleanup,
                props = props,
                @private = options.isPrivate,
                temporaryMembership = options.temporaryMembership
            });
            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<ChannelResponse>(json);
            return res.channel;
        }

        public async Task<Channel> Create(CreateChannelOptions options)
        {
            options ??= new CreateChannelOptions();
            return await SendCreateChannelRequest(options);
        }

        [Obsolete("Use Create(CreateChannelOptions options) instead.")]
        public async Task<Channel> Create(string name, bool autoCleanup = false, params (string, string)[] propTuples)
        {
            var options = new CreateChannelOptions
            {
                name = name,
                autoCleanup = autoCleanup,
                props = propTuples,
                isPrivate = false
            };
            return await SendCreateChannelRequest(options);
        }

        [Obsolete("Use Create(CreateChannelOptions options) instead.")]
        public async Task<Channel> CreatePrivate(string name, bool autoCleanup = false, params (string, string)[] propTuples)
        {
            var options = new CreateChannelOptions
            {
                name = name,
                autoCleanup = autoCleanup,
                props = propTuples,
                isPrivate = true
            };
            return await SendCreateChannelRequest(options);
        }

        public async Task<Channel> Join(int channelId)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/{channelId}/join");
            var json = await Call(uri, "POST");

            var res = JsonUtility.FromJson<ChannelResponse>(json);
            return res.channel;
        }

        public async Task Leave(int channelId)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/{channelId}/leave");
            await Call(uri, "POST");
        }

        public async Task<Channel> Update(int channelId, string name = "", int newOwnerAliasId = -1, params (string, string)[] propTuples)
        {
            Talo.IdentityCheck();

            var props = propTuples.Select((propTuple) => new Prop(propTuple)).ToArray();

            var uri = new Uri($"{baseUrl}/{channelId}");

            var content = "";
            if (newOwnerAliasId == -1)
            {
                content = JsonUtility.ToJson(new ChannelsUpdateRequest { name = name, props = props });
            }
            else
            {
                content = JsonUtility.ToJson(new ChannelsUpdateOwnerRequest { name = name, newOwnerAliasId = newOwnerAliasId, props = props });
            }
            var json = await Call(uri, "PUT", content);

            var res = JsonUtility.FromJson<ChannelResponse>(json);
            return res.channel;
        }

        public async Task Delete(int channelId)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/{channelId}");
            await Call(uri, "DELETE");
        }

        public void SendMessage(int channelId, string message)
        {
            Talo.IdentityCheck();

            var payload = new ChannelMessageRequest
            {
                channel = new ChannelMessageRequest.ChannelStub { id = channelId },
                message = message
            };

            Talo.Socket.Send(new SocketRequest<ChannelMessageRequest>("v1.channels.message", payload));
        }

        public async Task<Channel> Find(int channelId)
        {
            var uri = new Uri($"{baseUrl}/{channelId}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<ChannelResponse>(json);
            return res.channel;
        }

        public async Task Invite(int channelId, int playerAliasId)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/{channelId}/invite");
            var content = JsonUtility.ToJson(new ChannelsInviteRequest { inviteeAliasId = playerAliasId });
            await Call(uri, "POST", content);
        }

        public async Task<PlayerAlias[]> GetMembers(int channelId)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/{channelId}/members");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<ChannelsMembersResponse>(json);
            return res.members;
        }

        public async Task<ChannelStorageProp> GetStorageProp(int channelId, string propKey, bool bustCache = false)
        {
            Talo.IdentityCheck();

            if (!bustCache)
            {
                return await _storageManager.GetProp(channelId, propKey);
            }

            var uri = new Uri($"{baseUrl}/{channelId}/storage?propKey={propKey}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<ChannelStoragePropGetResponse>(json);
            if (res.prop == null)
            {
                return null;
            }

            _storageManager.UpsertProp(channelId, res.prop);
            return res.prop;
        }

        public async Task SetStorageProps(int channelId, params (string, string)[] propTuples)
        {
            Talo.IdentityCheck();

            var props = propTuples.Select((propTuple) => new Prop(propTuple)).ToArray();
            var content = JsonUtility.ToJson(new ChannelStoragePropsSetRequest { props = props });

            var uri = new Uri($"{baseUrl}/{channelId}/storage");
            var json = await Call(uri, "PUT", content);

            var res = JsonUtility.FromJson<ChannelStoragePropsSetResponse>(json);
            if (res.failedProps.Length > 0)
            {
                OnChannelStoragePropsFailedToSet?.Invoke(res.channel, res.failedProps);
            }
        }
    }
}
