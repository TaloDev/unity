using System.IO;
using TaloGameServices;
using UnityEngine;

public class SavesFileHandler : IFileHandler<OfflineSavesContent>
{
    public OfflineSavesContent ReadContent(string path)
    {
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<OfflineSavesContent>(Talo.Crypto.ReadFileContent(path));
    }

    public void WriteContent(string path, OfflineSavesContent content)
    {
        Talo.Crypto.WriteFileContent(path, JsonUtility.ToJson(content));
    }
}
