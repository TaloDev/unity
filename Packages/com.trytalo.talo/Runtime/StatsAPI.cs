using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class StatsAPI : BaseAPI
    {
        public StatsAPI(TaloManager manager) : base(manager, "game-stats") { }

        public async Task Track(string internalName, float change = 1f)
        {
            Talo.IdentityCheck();

            var uri = new Uri(baseUrl + $"/{internalName}");
            var content = JsonUtility.ToJson(new StatsPutRequest() { change = change });

            await Call(uri, "PUT", content);
        }
    }
}
