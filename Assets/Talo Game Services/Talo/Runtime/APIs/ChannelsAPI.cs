using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class ChannelsAPI: BaseAPI
    {
        public event Action<Channel, PlayerAlias, string> OnMessageReceived;
        public event Action<Channel, PlayerAlias> OnChannelJoined;
        public event Action<Channel, PlayerAlias> OnChannelLeft;
        public event Action<Channel, PlayerAlias> OnOwnershipTransferred;
        public event Action<Channel> OnChannelDeleted;
        public event Action<Channel, string[]> OnChannelUpdated;

        public ChannelsAPI() : base("v1/game-channels") {
            Talo.Socket.OnMessageReceived += (response) => {
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
                    OnChannelLeft?.Invoke(data.channel, data.playerAlias);
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
            };
        }

        public async Task<ChannelsIndexResponse> GetChannels(int page)
        {
            var uri = new Uri($"{baseUrl}?page={page}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<ChannelsIndexResponse>(json);
            return res;
        }

        public async Task<Channel[]> GetSubscribedChannels()
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/subscriptions");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<ChannelsSubscriptionsResponse>(json);
            return res.channels;
        }

        private async Task<Channel> SendCreateChannelRequest(
            string name,
            bool autoCleanup = false,
            bool isPrivate = false,
            params (string, string)[] propTuples
        )
        {
            Talo.IdentityCheck();

            var props = propTuples.Select((propTuple) => new Prop(propTuple)).ToArray();

            var uri = new Uri(baseUrl);
            var content = JsonUtility.ToJson(new ChannelsCreateRequest {
                name = name,
                autoCleanup = autoCleanup,
                props = props,
                @private = isPrivate
            });
            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<ChannelResponse>(json);
            return res.channel;
        }

        public async Task<Channel> Create(string name, bool autoCleanup = false, params (string, string)[] propTuples)
        {
            return await SendCreateChannelRequest(name, autoCleanup, false, propTuples);
        }

        public async Task<Channel> CreatePrivate(string name, bool autoCleanup = false, params (string, string)[] propTuples)
        {
            return await SendCreateChannelRequest(name, autoCleanup, true, propTuples);
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
    }
}
