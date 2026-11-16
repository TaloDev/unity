using System;
using UnityEngine;

namespace TaloGameServices
{
    [Serializable]
    public struct ClaimKeyEntry
    {
        public string id;
        public string claimKey;
    }

    [Serializable]
    public class ClaimKeysContent
    {
        public ClaimKeyEntry[] players;
        public ClaimKeyEntry[] identities;
    }

    public class ClaimKeyManager
    {
        public const string Header = "X-Talo-Claim-Key";

        private readonly string _path = Application.persistentDataPath + "/tck.bin";

        private readonly IFileHandler<ClaimKeysContent> _fileHandler;

        private string _currentPlayerId;
        private string _currentKey;

        public ClaimKeyManager()
        {
            _fileHandler = Talo.TestMode
                ? new ClaimKeysTestFileHandler()
                : new ClaimKeysFileHandler();
        }

        public string GetCurrentKey(Player player)
        {
            if (player == null)
            {
                return "";
            }

            if (_currentPlayerId != player.id)
            {
                _currentPlayerId = player.id;
                _currentKey = GetPlayerKey(player.id);
            }

            return _currentKey;
        }

        public void ClearCurrentKey()
        {
            _currentPlayerId = null;
            _currentKey = null;
        }

        public string GetPlayerKey(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return "";
            }

            return Find(ReadContent().players, playerId);
        }

        public string GetIdentifyKey(string service, string identifier)
        {
            return Find(ReadContent().identities, IdentityKey(service, identifier));
        }

        public void SaveKey(string playerId, string claimKey, string service = "", string identifier = "")
        {
            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(claimKey))
            {
                return;
            }

            var content = ReadContent();
            var identityKey = string.IsNullOrEmpty(service) || string.IsNullOrEmpty(identifier)
                ? ""
                : IdentityKey(service, identifier);

            content.players = Upsert(content.players, playerId, claimKey);

            if (identityKey != "")
            {
                content.identities = Upsert(content.identities, identityKey, claimKey);
            }

            _fileHandler.WriteContent(_path, content);
            ClearCurrentKey();
        }

        public void ForgetPlayer(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return;
            }

            var content = ReadContent();
            var remaining = Array.FindAll(content.players, (entry) => entry.id != playerId);
            if (remaining.Length == content.players.Length)
            {
                return;
            }

            content.players = remaining;
            _fileHandler.WriteContent(_path, content);
            ClearCurrentKey();
        }

        private ClaimKeysContent ReadContent()
        {
            var content = _fileHandler.ReadContent(_path) ?? new ClaimKeysContent();
            content.players ??= Array.Empty<ClaimKeyEntry>();
            content.identities ??= Array.Empty<ClaimKeyEntry>();

            return content;
        }

        private static string IdentityKey(string service, string identifier)
        {
            return $"{service.Trim().ToLowerInvariant()}:{identifier.Trim()}";
        }

        private static string Find(ClaimKeyEntry[] entries, string id)
        {
            foreach (var entry in entries)
            {
                if (entry.id == id)
                {
                    return entry.claimKey;
                }
            }

            return "";
        }

        private static ClaimKeyEntry[] Upsert(ClaimKeyEntry[] entries, string id, string claimKey)
        {
            var updated = (ClaimKeyEntry[])entries.Clone();
            var index = Array.FindIndex(updated, (entry) => entry.id == id);

            if (index == -1)
            {
                Array.Resize(ref updated, updated.Length + 1);
                index = updated.Length - 1;
                updated[index].id = id;
            }

            updated[index].claimKey = claimKey;
            return updated;
        }
    }
}
