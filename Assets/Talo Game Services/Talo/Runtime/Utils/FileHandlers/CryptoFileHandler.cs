using System.IO;

namespace TaloGameServices
{
    public class CryptoFileHandler : IFileHandler<string>
    {
        public string ReadContent(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteContent(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}
