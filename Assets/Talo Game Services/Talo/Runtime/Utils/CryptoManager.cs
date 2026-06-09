using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TaloGameServices
{
    [Serializable]
    internal struct SignatureHeader
    {
        public string rid;
        public string payload;
        public long timestamp;
    }

    public class CryptoManager
    {
        private readonly string _keyPath = Application.persistentDataPath + "/ti.bin";

        private readonly IFileHandler<string> _fileHandler;

        public CryptoManager()
        {
            _fileHandler = Talo.TestMode
                ? new CryptoTestFileHandler()
                : new CryptoFileHandler();

            if (!File.Exists(_keyPath) || Talo.TestMode)
            {
                var randomBytes = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }

                var encryptedKey = EncryptString(GetAESKey(), BitConverter.ToString(randomBytes).Replace("-", ""));
                _fileHandler.WriteContent(_keyPath, encryptedKey);
            }
        }

        private byte[] GetUniqueKey()
        {
            var encryptedKey = _fileHandler.ReadContent(_keyPath);
            var decryptedKeyHex = DecryptString(GetAESKey(), encryptedKey);
            return HexStringToByteArray(decryptedKeyHex);
        }

        public string ReadFileContent(string path)
        {
            return DecryptString(GetUniqueKey(), _fileHandler.ReadContent(path));
        }

        public void WriteFileContent(string path, string content)
        {
            _fileHandler.WriteContent(path, EncryptString(GetUniqueKey(), content));
        }

        private byte[] GetAESKey()
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier));
        }

        private string EncryptString(byte[] key, string text)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
            using StreamWriter sw = new(cs);

            sw.Write(text);
            sw.Flush();
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        private string DecryptString(byte[] key, string text)
        {
            var fullCipher = Convert.FromBase64String(text);

            using var aes = Aes.Create();
            aes.Key = key;

            var iv = new byte[aes.BlockSize / 8];
            Array.Copy(fullCipher, iv, iv.Length);

            var cipherBytes = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        private byte[] HexStringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];

            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static string CreateRequestSignature(string requestBody)
        {
            var keyVersion = Talo.Settings.verificationKeyVersion;
            var keyValue = Talo.Settings.verificationKeyValue;

            if (string.IsNullOrEmpty(keyVersion) || string.IsNullOrEmpty(keyValue))
            {
                throw new InvalidOperationException("Verification is enabled but verificationKeyVersion or verificationKeyValue is missing. Please update your Talo Settings.");
            }

            var rid = Guid.NewGuid().ToString();

            using var sha256 = SHA256.Create();
            var bodyHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
            var payload = BitConverter.ToString(bodyHashBytes).Replace("-", "").ToLower();

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var headerJson = JsonUtility.ToJson(new SignatureHeader {
                rid = rid,
                payload = payload,
                timestamp = timestamp
            });
            var headerBytes = Encoding.UTF8.GetBytes(headerJson);
            var headerB64 = Convert.ToBase64String(headerBytes);

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keyValue));
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(headerB64));
            var signatureB64 = Convert.ToBase64String(signatureBytes);

            return $"{keyVersion}|{headerB64}.{signatureB64}";
        }
    }
}
