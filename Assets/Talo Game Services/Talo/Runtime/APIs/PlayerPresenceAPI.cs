using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class PlayerPresenceAPI : BaseAPI
    {
        public event Action<PlayerPresence, bool, bool> OnPresenceChanged;

        public PlayerPresenceAPI() : base("v1/players/presence") 
        {
            Talo.Socket.OnMessageReceived += (response) => {
                if (response.GetResponseType() == "v1.players.presence.updated")
                {
                    var data = response.GetData<PlayerPresenceUpdatedResponse>();
                    OnPresenceChanged?.Invoke(
                        data.presence,
                        data.meta.onlineChanged,
                        data.meta.customStatusChanged
                    );
                }
            };
        }

        public async Task<PlayerPresence> GetPresence(string playerId)
        {
            var uri = new Uri($"{baseUrl}/{playerId}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<PresenceResponse>(json);
            return res.presence;
        }

        public async Task<PlayerPresence> UpdatePresence(bool online, string customStatus = "")
        {
            Talo.IdentityCheck();

            var uri = new Uri(baseUrl);
            var content = JsonUtility.ToJson(new PlayerPresenceUpdateRequest
            {
                online = online,
                customStatus = customStatus
            });

            var json = await Call(uri, "PUT", content);
            var res = JsonUtility.FromJson<PresenceResponse>(json);
            return res.presence;
        }
    }
}
