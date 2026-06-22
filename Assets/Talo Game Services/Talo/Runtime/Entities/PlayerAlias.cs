using System.IO;
using UnityEngine;

namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAlias
    {
        private static readonly string offlineDataPath = Application.persistentDataPath + "/ta.bin";

        public int id;
        public string service, identifier, displayName;
        public Player player;
        public string lastSeenAt, createdAt, updatedAt;

        public bool MatchesIdentifyRequest(string service, string identifier)
        {
            return this.service == service && this.identifier == identifier;
        }

        public void WriteOfflineAlias()
        {
            if (!Talo.Settings.cachePlayerOnIdentify)
            {
                return;
            }

            var content = JsonUtility.ToJson(this);
            Talo.Crypto.WriteFileContent(offlineDataPath, content);
        }

        public static bool HasOfflineAlias()
        {
            return Talo.Settings.cachePlayerOnIdentify && File.Exists(offlineDataPath);
        }

        public static PlayerAlias GetOfflineAlias()
        {
            if (!HasOfflineAlias())
            {
                return null;
            }

            return JsonUtility.FromJson<PlayerAlias>(Talo.Crypto.ReadFileContent(offlineDataPath));
        }

        public static void DeleteOfflineAlias()
        {
            if (File.Exists(offlineDataPath))
            {
                try
                {
                    File.Delete(offlineDataPath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to delete offline player data: {ex.Message}");
                }
            }
        }
    }
}
