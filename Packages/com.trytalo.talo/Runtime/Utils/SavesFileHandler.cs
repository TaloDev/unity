using System.IO;
using TaloGameServices;
using UnityEngine;

public class SavesFileHandler : IFileHandler<OfflineSavesContent>
{
    public OfflineSavesContent ReadContent(string path)
    {
        if (!File.Exists(path)) return null;

        var sr = new StreamReader(path);
        var content = sr.ReadToEnd();
        sr.Close();

        return JsonUtility.FromJson<OfflineSavesContent>(content);
    }

    public void WriteContent(string path, OfflineSavesContent content)
    {
        var sw = new StreamWriter(path);
        sw.WriteLine(JsonUtility.ToJson(content));
        sw.Close();
    }
}
