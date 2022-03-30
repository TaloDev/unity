using System;
using System.Net.Http;
using System.Text;
using TaloGameServices;
using UnityEngine;

public class StatsAPI : BaseAPI
{
    public StatsAPI(TaloSettings settings, HttpClient client) : base(settings, client, "game-stats") { }

    public async void Track(string internalName, float change = 1f)
    {
        Talo.IdentityCheck();

        var req = new HttpRequestMessage();
        req.Method = HttpMethod.Post;
        req.RequestUri = new Uri(baseUrl + $"/{internalName}");

        string content = JsonUtility.ToJson(new StatsPutRequest(change));
        req.Content = new StringContent(content, Encoding.UTF8, "application/json");

        await Call(req);
    }
}
