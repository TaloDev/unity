using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class RequestBodyEncoderTest
    {
        // every gzip stream starts with 0x1F 0x8B
        private const byte FirstGzipByte = 0x1F;
        private const byte SecondGzipByte = 0x8B;

        private static readonly UTF8Encoding Utf8 = new();

        private static string BuildJson(int size)
        {
            var padding = new string('a', size);

            return $"{{\"name\":\"event\",\"props\":[{{\"key\":\"pad\",\"value\":\"{padding}\"}}]}}";
        }

        private static byte[] Decompress(byte[] body)
        {
            using (var input = new MemoryStream(body))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }

        [Test]
        public void Encode_EmptyContent_ReturnsNoBytes()
        {
            var body = RequestBodyEncoder.Encode("", true, out var gzipped);

            Assert.AreEqual(0, body.Length);
            Assert.IsFalse(gzipped);
        }

        [Test]
        public void Encode_SmallBody_IsNotCompressed()
        {
            var content = BuildJson(100);

            var body = RequestBodyEncoder.Encode(content, true, out var gzipped);

            Assert.IsFalse(gzipped);
            CollectionAssert.AreEqual(Utf8.GetBytes(content), body);
        }

        [Test]
        public void Encode_LargeBody_StartsWithGzipHeader()
        {
            var body = RequestBodyEncoder.Encode(BuildJson(4096), true, out var gzipped);

            Assert.IsTrue(gzipped);
            Assert.Greater(body.Length, 2);
            Assert.AreEqual(FirstGzipByte, body[0]);
            Assert.AreEqual(SecondGzipByte, body[1]);
        }

        [Test]
        public void Encode_LargeBody_DecompressesToOriginal()
        {
            var content = BuildJson(4096);

            var body = RequestBodyEncoder.Encode(content, true, out _);

            CollectionAssert.AreEqual(Utf8.GetBytes(content), Decompress(body));
        }

        [Test]
        public void Encode_NonAsciiBody_DecompressesToOriginal()
        {
            // 2000 two-byte chars and an emoji: wrong encoding would break the round trip
            var content = $"{{\"text\":\"{new string('\u00fc', 2000)} \U0001F600\"}}";

            var body = RequestBodyEncoder.Encode(content, true, out var gzipped);

            Assert.IsTrue(gzipped);
            CollectionAssert.AreEqual(Utf8.GetBytes(content), Decompress(body));
        }

        [Test]
        public void Encode_CompressDisabled_IsNotCompressed()
        {
            var content = BuildJson(4096);

            var body = RequestBodyEncoder.Encode(content, false, out var gzipped);

            Assert.IsFalse(gzipped);
            CollectionAssert.AreEqual(Utf8.GetBytes(content), body);
        }

        [Test]
        public void Encode_RepeatedEvents_IsSmallerThanInput()
        {
            var eventJson = BuildJson(380);
            var content = $"[{string.Join(",", Enumerable.Repeat(eventJson, 50))}]";

            var body = RequestBodyEncoder.Encode(content, true, out var gzipped);

            Assert.IsTrue(gzipped);
            Assert.Less(body.Length, content.Length);
        }
    }
}
