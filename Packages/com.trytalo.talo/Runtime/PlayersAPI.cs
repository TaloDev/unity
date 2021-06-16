using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices {
    public class PlayersAPI : BaseAPI {
        public PlayersAPI(TaloSettings settings, HttpClient client) : base(settings, client, "players") { }

        public async void Create(PlayerAlias alias) {
            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(baseUrl);
            req.Content = new StringContent(JsonUtility.ToJson(alias), Encoding.UTF8, "application/json");

            string json = await Call(req);
            var res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);

            Talo.CurrentPlayer = res.alias;
        }

        public async Task Identify(string service, string identifier) {
            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(baseUrl + $"/identify?service={service}&identifier={identifier}");

            string json = await Call(req);
            var res = JsonUtility.FromJson<PlayersIdentifyResponse>(json);
            Talo.CurrentPlayer = res.alias;
        }
    }
}
