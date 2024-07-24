using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class LeaderboardsAPI : BaseAPI
    {
        public LeaderboardsAPI(TaloManager manager) : base(manager, "v1/leaderboards") { }

        public async Task<LeaderboardEntriesResponse> GetEntries(string internalName, int page)
        {
            var uri = new Uri(baseUrl + $"/{internalName}/entries?page={page}");

            var json = await Call(uri, "GET");
            var res = JsonUtility.FromJson<LeaderboardEntriesResponse>(json);
            return res;
        }

        public async Task<LeaderboardEntriesResponse> GetEntriesForCurrentPlayer(string internalName, int page)
        {
            Talo.IdentityCheck();

            var uri = new Uri(baseUrl + $"/{internalName}/entries?page={page}&aliasId={Talo.CurrentAlias.id}");

            var json = await Call(uri, "GET");
            var res = JsonUtility.FromJson<LeaderboardEntriesResponse>(json);
            return res;
        }

        public async Task<(LeaderboardEntry, bool)> AddEntry(string internalName, float score)
        {
            Talo.IdentityCheck();

            var uri = new Uri(baseUrl + $"/{internalName}/entries");
            var content = JsonUtility.ToJson(new LeaderboardsPostRequest { score = score });

            var json = await Call(uri, "POST", content);
            var res = JsonUtility.FromJson<LeaderboardEntryResponse>(json);

            return (res.entry, res.updated);
        }
    }
}
