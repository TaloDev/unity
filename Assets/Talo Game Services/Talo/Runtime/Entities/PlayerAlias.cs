using System.IO;
using UnityEngine;

namespace TaloGameServices
{
    [System.Serializable]
    public class PlayerAlias
    {
        private static string _offlineDataPath;
        private static string OfflineDataPath =>
            _offlineDataPath ??= Application.persistentDataPath + "/ta.bin";

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
            Talo.Crypto.WriteFileContent(OfflineDataPath, content);
        }

        public static bool HasOfflineAlias()
        {
            return Talo.Settings.cachePlayerOnIdentify && File.Exists(OfflineDataPath);
        }

        public static PlayerAlias GetOfflineAlias()
        {
            if (!HasOfflineAlias())
            {
                return null;
            }

            return JsonUtility.FromJson<PlayerAlias>(Talo.Crypto.ReadFileContent(OfflineDataPath));
        }

        public static void DeleteOfflineAlias()
        {
            if (File.Exists(OfflineDataPath))
            {
                try
                {
                    File.Delete(OfflineDataPath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to delete offline player data: {ex.Message}");
                }
            }
        }
    }
}
