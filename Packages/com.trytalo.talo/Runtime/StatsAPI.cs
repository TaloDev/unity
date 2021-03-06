using System;
using TaloGameServices;
using UnityEngine;

public class StatsAPI : BaseAPI
{
    public StatsAPI(TaloManager manager) : base(manager, "game-stats") { }

    public async void Track(string internalName, float change = 1f)
    {
        Talo.IdentityCheck();

        var uri = new Uri(baseUrl + $"/{internalName}");
        var content = JsonUtility.ToJson(new StatsPutRequest(change));

        await Call(uri, "PUT", content);
    }
}
