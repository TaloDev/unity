using System;
using System.IO;
using UnityEngine;

namespace TaloGameServices
{
    public class ClaimKeysFileHandler : IFileHandler<ClaimKeysContent>
    {
        public ClaimKeysContent ReadContent(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<ClaimKeysContent>(Talo.Crypto.ReadFileContent(path));
            }
            catch (Exception ex)
            {
                // an unreadable store must not fail every request
                Debug.LogWarning($"Failed to read the claim key store: {ex.Message}");
                return null;
            }
        }

        public void WriteContent(string path, ClaimKeysContent content)
        {
            Talo.Crypto.WriteFileContent(path, JsonUtility.ToJson(content));
        }
    }
}
