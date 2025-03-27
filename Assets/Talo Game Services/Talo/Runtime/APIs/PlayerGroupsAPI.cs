using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class PlayerGroupsAPI : BaseAPI
    {
        public PlayerGroupsAPI() : base("v1/player-groups") { }

        public async Task<Group> Get(string groupId)
        {
            var uri = new Uri($"{baseUrl}/{groupId}");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<PlayerGroupsGetResponse>(json);
            return res.group;
        }
    }
}
