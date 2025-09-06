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
                    SetLiveConfig(new LiveConfig(data.config), true);
                }
            };
        }

        public async Task<LiveConfig> Get()
        {
            if (Talo.IsOffline())
            {
                return GetOfflineConfig();
            }

            var uri = new Uri(baseUrl);
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<GameConfigResponse>(json);
            SetLiveConfig(new LiveConfig(res.config));
            return Talo.LiveConfig;
        }

        public LiveConfig GetOfflineConfig()
        {
            var offlineConfig = LiveConfig.GetOfflineConfig();
            if (offlineConfig != null)
            {
                SetLiveConfig(offlineConfig);
            }
            return offlineConfig;
        }

        private void SetLiveConfig(LiveConfig liveConfig, bool isUpdate = false)
        {
            Talo.LiveConfig = liveConfig;
            liveConfig.WriteOfflineConfig();
            
            if (isUpdate)
            {
                OnLiveConfigUpdated?.Invoke(liveConfig);
            }
            else
            {
                OnLiveConfigLoaded?.Invoke(liveConfig);
            }
        }
    }
}
