using System;
using UnityEngine;

namespace TaloGameServices
{
    public class GameConfigAPI : BaseAPI
    {
        public event Action<LiveConfig> OnLiveConfigLoaded;

        public GameConfigAPI(TaloManager manager) : base(manager, "game-config") { }

        public async void Get()
        {
            var uri = new Uri(baseUrl);
            var json = await Call(uri, "GET");
            var res = JsonUtility.FromJson<GameConfigResponse>(json);

            Talo.LiveConfig = new LiveConfig(res.config);
            OnLiveConfigLoaded?.Invoke(Talo.LiveConfig);
        }
    }
}
