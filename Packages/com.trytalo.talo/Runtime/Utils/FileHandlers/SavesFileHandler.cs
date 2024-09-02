using System.IO;
using TaloGameServices;
using UnityEngine;

public class SavesFileHandler : IFileHandler<OfflineSavesContent>
{
    public OfflineSavesContent ReadContent(string path)
    {
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<OfflineSavesContent>(File.ReadAllText(path));
    }

    public void WriteContent(string path, OfflineSavesContent content)
    {
        File.WriteAllText(path, JsonUtility.ToJson(content));
    }
}
