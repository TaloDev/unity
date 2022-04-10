using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class PlayersAPI : BaseAPI
    {
        public PlayersAPI(TaloManager manager) : base(manager, "players") { }

        public async void Create(PlayerAlias alias)
        {
            var uri = new Uri(baseUrl);
            var content = JsonUtility.ToJson(alias);

            var json = await Call(uri, "POST", content);
            var res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);

            Talo.CurrentAlias = res.alias;
        }

        public async Task Identify(string service, string identifier)
        {
            var uri = new Uri(baseUrl + $"/identify?service={service}&identifier={identifier}");

            var json = await Call(uri, "GET");
            var res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);

            Talo.CurrentAlias = res.alias;

            await Talo.Saves.GetSaves();
        }

        public async void Update()
        {
            var uri = new Uri(baseUrl + $"/{Talo.CurrentPlayer.id}");
            var content = JsonUtility.ToJson(Talo.CurrentPlayer);

            var json = await Call(uri, "PATCH", content);
            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);

            Talo.CurrentPlayer = res.player;
        }

        public async Task<Player> Merge(string alias1, string alias2)
        {
            var uri = new Uri(baseUrl + "/merge");
            string content = JsonUtility.ToJson(new PlayersMergeRequest(alias1, alias2));

            string json = await Call(uri, "POST", content);
            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);

            return res.player;
        }
    }
}
