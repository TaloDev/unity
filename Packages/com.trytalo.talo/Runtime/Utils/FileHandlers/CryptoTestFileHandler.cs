using System.Collections.Generic;
using UnityEngine;

internal class CryptoTestFileHandler : IFileHandler<string>
{
    private Dictionary<string, string> files = new ();

    public string ReadContent(string path)
    {
        if (!files.ContainsKey(path)) return "";
        return files[path];
    }

    public void WriteContent(string path, string content)
    {
        files[path] = content;
    }
}
