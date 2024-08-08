using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class LeaderboardsAPI : BaseAPI
    {
        private LeaderboardEntriesManager _entriesManager = new();

        public LeaderboardsAPI(TaloManager manager) : base(manager, "v1/leaderboards") { }

        public List<LeaderboardEntry> GetCachedEntries(string internalName)
        {
            return _entriesManager.GetEntries(internalName);
        }

        public List<LeaderboardEntry> GetCachedEntriesForCurrentPlayer(string internalName)
        {
            Talo.IdentityCheck();

            return _entriesManager.GetEntries(internalName).FindAll(e => e.playerAlias.id == Talo.CurrentAlias.id);
        }

        public async Task<LeaderboardEntriesResponse> GetEntries(string internalName, int page)
        {
            var uri = new Uri(baseUrl + $"/{internalName}/entries?page={page}");

            var json = await Call(uri, "GET");
            var res = JsonUtility.FromJson<LeaderboardEntriesResponse>(json);

            foreach (var entry in res.entries)
            {
                _entriesManager.UpsertEntry(internalName, entry);
            }

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

            _entriesManager.UpsertEntry(internalName, res.entry);

            return (res.entry, res.updated);
        }
    }
}
