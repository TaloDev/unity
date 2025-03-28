using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class GameConfigAPI : BaseAPI
    {
        public event Action<LiveConfig> OnLiveConfigLoaded;
        public event Action<LiveConfig> OnLiveConfigUpdated;

        public GameConfigAPI() : base("v1/game-config") {
            Talo.Socket.OnMessageReceived += (response) => {
                if (response.GetResponseType() == "v1.live-config.updated")
                {
                    var data = response.GetData<GameConfigResponse>();
                    OnLiveConfigUpdated?.Invoke(new LiveConfig(data.config));
                }
            };
        }

        public async Task<LiveConfig> Get()
        {
            var uri = new Uri(baseUrl);
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<GameConfigResponse>(json);
            Talo.LiveConfig = new LiveConfig(res.config);
            OnLiveConfigLoaded?.Invoke(Talo.LiveConfig);

            return Talo.LiveConfig;
        }
    }
}
