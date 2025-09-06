using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TaloGameServices
{
    public class LiveConfig
    {
        private static readonly string offlineDataPath = Application.persistentDataPath + "/tlc.bin";
        private readonly Prop[] props;

        public LiveConfig(Prop[] props)
        {
            this.props = props;
        }

        public T GetProp<T>(string key, T fallback = default)
        {
            try
            {
                Prop prop = props.First((prop) => prop.key == key);
                return (T)Convert.ChangeType(prop.value, typeof(T));
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        public void WriteOfflineConfig()
        {
            try
            {
                var json = JsonUtility.ToJson(new GameConfigResponse { config = props });
                Talo.Crypto.WriteFileContent(offlineDataPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to write offline config: {ex.Message}");
            }
        }

        public static LiveConfig GetOfflineConfig()
        {
            try
            {
                if (!File.Exists(offlineDataPath))
                {
                    return null;
                }

                var json = Talo.Crypto.ReadFileContent(offlineDataPath);
                var response = JsonUtility.FromJson<GameConfigResponse>(json);
                return new LiveConfig(response.config);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to read offline config: {ex.Message}");
                return null;
            }
        }

        public static long GetOfflineConfigLastModified()
        {
            try
            {
                if (!File.Exists(offlineDataPath))
                {
                    return 0;
                }

                var fileInfo = new FileInfo(offlineDataPath);
                return new DateTimeOffset(fileInfo.LastWriteTimeUtc).ToUnixTimeSeconds();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get offline config last modified time: {ex.Message}");
                return 0;
            }
        }
    }
}
