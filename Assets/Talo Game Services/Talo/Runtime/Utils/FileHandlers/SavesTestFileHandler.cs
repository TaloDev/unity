using UnityEngine;

namespace TaloGameServices
{
    internal class SavesTestFileHandler : IFileHandler<OfflineSavesContent>
    {
        private string content;

        public OfflineSavesContent ReadContent(string path)
        {
            if (content == default) return null;
            return JsonUtility.FromJson<OfflineSavesContent>(content);
        }

        public void WriteContent(string path, OfflineSavesContent content)
        {
            this.content = JsonUtility.ToJson(content);
        }
    }
}
