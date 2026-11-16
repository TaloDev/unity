using System.Collections.Generic;
using UnityEngine;

namespace TaloGameServices
{
    internal class ClaimKeysTestFileHandler : IFileHandler<ClaimKeysContent>
    {
        private static readonly Dictionary<string, string> _files = new();

        public static void Reset()
        {
            _files.Clear();
        }

        public ClaimKeysContent ReadContent(string path)
        {
            if (!_files.TryGetValue(path, out var content))
            {
                return null;
            }

            return JsonUtility.FromJson<ClaimKeysContent>(content);
        }

        public void WriteContent(string path, ClaimKeysContent content)
        {
            _files[path] = JsonUtility.ToJson(content);
        }
    }
}
