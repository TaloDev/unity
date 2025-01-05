using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class ChannelsAPI: BaseAPI
    {
        public ChannelsAPI() : base("v1/game-channels") { }

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

        public async Task<Channel> Create(string name, bool autoCleanup = false, params (string, string)[] propTuples)
        {
            Talo.IdentityCheck();

            var props = propTuples.Select((propTuple) => new Prop(propTuple)).ToArray();

            var uri = new Uri(baseUrl);
            var content = JsonUtility.ToJson(new ChannelsCreateRequest { name = name, autoCleanup = autoCleanup, props = props });
            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<ChannelResponse>(json);
            return res.channel;
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
    }
}
