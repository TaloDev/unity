using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class PlayerGroupsAPI : BaseAPI
    {
        public PlayerGroupsAPI() : base("v1/player-groups") { }

        public async Task<PlayerGroupsGetResponse> Get(string groupId, int page = 0)
        {
            var uri = new Uri($"{baseUrl}/{groupId}?page={page}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<PlayerGroupsGetResponse>(json);
            return res;
        }
    }
}
