using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class LeaderboardsAPI : BaseAPI
    {
        public LeaderboardsAPI(TaloSettings settings, HttpClient client) : base(settings, client, "leaderboards") { }

        public async Task<LeaderboardEntry[]> GetEntries(string internalName, int page)
        {
            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(baseUrl + $"/{internalName}/entries?page={page}");

            string json = await Call(req);
            var res = JsonUtility.FromJson<LeaderboardEntriesResponse>(json);
            return res.entries;
        }

        public async Task<LeaderboardEntry[]> GetEntriesForCurrentPlayer(string leaderboardInternalName, int page)
        {
            Talo.IdentityCheck();

            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(baseUrl + $"/{leaderboardInternalName}/entries?page={page}&aliasId={Talo.CurrentAlias.id}");

            string json = await Call(req);
            var res = JsonUtility.FromJson<LeaderboardEntriesResponse>(json);
            return res.entries;
        }

        public async Task<(LeaderboardEntry, bool)> AddEntry(string internalName, float score)
        {
            Talo.IdentityCheck();

            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(baseUrl + $"/{internalName}/entries");

            string content = JsonUtility.ToJson(new LeaderboardsPostRequest(score));
            req.Content = new StringContent(content, Encoding.UTF8, "application/json");

            string json = await Call(req);
            var res = JsonUtility.FromJson<LeaderboardEntryResponse>(json);
            return (res.entry, res.updated);
        }
    }
}
