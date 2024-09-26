using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace TaloGameServices
{
    public class LeaderboardsAPI : BaseAPI
    {
        private LeaderboardEntriesManager _entriesManager = new();

        public LeaderboardsAPI() : base("v1/leaderboards") { }

        public List<LeaderboardEntry> GetCachedEntries(string internalName)
        {
            return _entriesManager.GetEntries(internalName);
        }

        public List<LeaderboardEntry> GetCachedEntriesForCurrentPlayer(string internalName)
        {
            Talo.IdentityCheck();

            return _entriesManager.GetEntries(internalName).FindAll(e => e.playerAlias.id == Talo.CurrentAlias.id);
        }

        public async Task<LeaderboardEntriesResponse> GetEntries(string internalName, int page, int aliasId = -1)
        {
            var uri = new Uri($"{baseUrl}/{internalName}/entries?page={page}" + (aliasId != -1 ? $"&aliasId={aliasId}" : ""));
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
            return await GetEntries(internalName, page, Talo.CurrentAlias.id);
        }

        public async Task<(LeaderboardEntry, bool)> AddEntry(string internalName, float score, params (string, string)[] propTuples)
        {
            Talo.IdentityCheck();

            var props = propTuples.Select((propTuple) => new Prop(propTuple)).ToArray();

            var uri = new Uri($"{baseUrl}/{internalName}/entries");
            var content = JsonUtility.ToJson(new LeaderboardsPostRequest { score = score, props = props });
            var json = await Call(uri, "POST", Prop.SanitiseJson(content));

            var res = JsonUtility.FromJson<LeaderboardEntryResponse>(json);
            _entriesManager.UpsertEntry(internalName, res.entry);

            return (res.entry, res.updated);
        }
    }
}
