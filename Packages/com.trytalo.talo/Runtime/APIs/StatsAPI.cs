using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class StatsAPI : BaseAPI
    {
        public StatsAPI() : base("v1/game-stats") { }

        public async Task<StatsPutResponse> Track(string internalName, float change = 1f)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/{internalName}");
            var content = JsonUtility.ToJson(new StatsPutRequest { change = change });

            var json = await Call(uri, "PUT", content);
            return JsonUtility.FromJson<StatsPutResponse>(json);
        }

        public async Task<StatsHistoryResponse> GetHistory(string internalName, int page = 0, DateTime? startDate = null, DateTime? endDate = null)
        {
            Talo.IdentityCheck();

            var queryParams = new List<string> { $"page={page}" };
            if (startDate.HasValue)
            {
                queryParams.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("O"))}");
            }
            if (endDate.HasValue)
            {
                queryParams.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("O"))}");
            }

            var queryString = string.Join("&", queryParams);
            var uri = new Uri($"{baseUrl}/{internalName}/history?{queryString}");

            var json = await Call(uri, "GET");
            return JsonUtility.FromJson<StatsHistoryResponse>(json);
        }
    }
}
