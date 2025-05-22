using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace TaloGameServices
{
    public class GetEntriesOptions
    {
        public int page = 0;
        public int aliasId = -1;
        public bool includeArchived = false;
        public string propKey = "";
        public string propValue = "";

        public string ToQueryString()
        {
            var query = new Dictionary<string, string> { ["page"] = page.ToString() };
            if (aliasId != -1) query["aliasId"] = aliasId.ToString();
            if (includeArchived) query["withDeleted"] = "1";
            if (!string.IsNullOrEmpty(propKey)) query["propKey"] = propKey;
            if (!string.IsNullOrEmpty(propValue)) query["propValue"] = propValue;

            return string.Join("&", query.Select((param) => $"{param.Key}={param.Value}"));
        }
    }

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

        public async Task<LeaderboardEntriesResponse> GetEntries(string internalName, GetEntriesOptions options = null)
        {
            options ??= new GetEntriesOptions();

            var uri = new Uri($"{baseUrl}/{internalName}/entries?{options.ToQueryString()}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<LeaderboardEntriesResponse>(json);

            foreach (var entry in res.entries)
            {
                _entriesManager.UpsertEntry(internalName, entry);
            }

            return res;
        }

        public async Task<LeaderboardEntriesResponse> GetEntriesForCurrentPlayer(string internalName, GetEntriesOptions options = null)
        {
            Talo.IdentityCheck();

            options ??= new GetEntriesOptions();
            options.aliasId = Talo.CurrentAlias.id;

            return await GetEntries(internalName, options);
        }

        [Obsolete("Use GetEntries(string internalName, GetEntriesOptions options) instead.")]
        public async Task<LeaderboardEntriesResponse> GetEntries(string internalName, int page, int aliasId = -1, bool includeArchived = false)
        {
            return await GetEntries(internalName, new GetEntriesOptions
            {
                page = page,
                aliasId = aliasId,
                includeArchived = includeArchived
            });
        }

        [Obsolete("Use GetEntriesForCurrentPlayer(string internalName, GetEntriesOptions options) instead.")]
        public async Task<LeaderboardEntriesResponse> GetEntriesForCurrentPlayer(string internalName, int page, bool includeArchived = false)
        {
            Talo.IdentityCheck();

            return await GetEntries(internalName, new GetEntriesOptions
            {
                page = page,
                aliasId = Talo.CurrentAlias.id,
                includeArchived = includeArchived
            }); 
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
