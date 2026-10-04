using System.IO;
using System.IO.Compression;
using System.Text;

namespace TaloGameServices
{
    public static class RequestBodyEncoder
    {
        public const int MinCompressBytes = 1024;

        public const string GzipEncoding = "gzip";

        public static byte[] Encode(string content, bool compress, out bool gzipped)
        {
            gzipped = false;

            if (string.IsNullOrEmpty(content))
            {
                return new byte[0];
            }

            var raw = new UTF8Encoding().GetBytes(content);

            if (!compress || raw.Length < MinCompressBytes)
            {
                return raw;
            }

            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Fastest))
                {
                    gzip.Write(raw, 0, raw.Length);
                }

                gzipped = true;
                return output.ToArray();
            }
        }
    }
}
