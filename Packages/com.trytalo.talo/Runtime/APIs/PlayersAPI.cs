using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class PlayersAPI : BaseAPI
    {
        public event Action<Player> OnIdentified;

        public PlayersAPI() : base("v1/players") { }

        public void InvokeIdentifiedEvent()
        {
            OnIdentified?.Invoke(Talo.CurrentPlayer);
        }

        public async Task Identify(string service, string identifier)
        {
            var uri = new Uri($"{baseUrl}/identify?service={service}&identifier={identifier}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);

            Talo.CurrentAlias = res.alias;
            Talo.Socket.SetSocketToken(res.socketToken);
            InvokeIdentifiedEvent();
        }

        public async Task IdentifySteam(string ticket, string identity = "")
        {
            if (string.IsNullOrEmpty(identity))
            {
                await Identify("steam", ticket);
            }
            else
            {
                await Identify("steam", $"{identity}:{ticket}");
            }
        }

        public async Task Update()
        {
            var uri = new Uri($"{baseUrl}/{Talo.CurrentPlayer.id}");
            var content = JsonUtility.ToJson(Talo.CurrentPlayer);
            var json = await Call(uri, "PATCH", Prop.SanitiseJson(content));

            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);
            Talo.CurrentPlayer = res.player;
        }

        public async Task<Player> Merge(string playerId1, string playerId2)
        {
            var uri = new Uri($"{baseUrl}/merge");
            string content = JsonUtility.ToJson(new PlayersMergeRequest(playerId1, playerId2));
            var json = await Call(uri, "POST", content);

            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);
            return res.player;
        }
    }
}
