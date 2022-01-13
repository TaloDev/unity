using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class PlayersAPI : BaseAPI
    {
        public PlayersAPI(TaloSettings settings, HttpClient client) : base(settings, client, "players") { }

        public async void Create(PlayerAlias alias)
        {
            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(baseUrl);
            req.Content = new StringContent(JsonUtility.ToJson(alias), Encoding.UTF8, "application/json");

            string json = await Call(req);
            var res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);

            Talo.CurrentAlias = res.alias;
        }

        public async Task Identify(string service, string identifier)
        {
            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(baseUrl + $"/identify?service={service}&identifier={identifier}");

            string json = await Call(req);
            var res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);
            Talo.CurrentAlias = res.alias;

            await Talo.Saves.GetSaves();
        }

        public async void Update()
        {
            var req = new HttpRequestMessage();
            req.Method = new HttpMethod("PATCH");
            req.RequestUri = new Uri(baseUrl + $"/{Talo.CurrentPlayer.id}");

            string content = JsonUtility.ToJson(Talo.CurrentPlayer);
            req.Content = new StringContent(content, Encoding.UTF8, "application/json");

            string json = await Call(req);
            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);
            Talo.CurrentPlayer = res.player;
        }

        public async Task<Player> Merge(string alias1, string alias2)
        {
            var req = new HttpRequestMessage();
            req.Method = new HttpMethod("POST");
            req.RequestUri = new Uri(baseUrl + "/merge");

            string content = JsonUtility.ToJson(new PlayersMergeRequest(alias1, alias2));
            req.Content = new StringContent(content, Encoding.UTF8, "application/json");

            string json = await Call(req);
            var res = JsonUtility.FromJson<PlayersUpdateResponse>(json);
            return res.player;
        }
    }
}
