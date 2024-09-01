using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TaloGameServices
{
    public class CryptoManager
    {
        private readonly string _keyPath = Application.persistentDataPath + "/talo-init.bin";

        public CryptoManager()
        {
            if (!File.Exists(_keyPath))
            {
                var randomBytes = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }

                var encryptedKey = EncryptString(GetAESKey(), BitConverter.ToString(randomBytes).Replace("-", ""));
                File.WriteAllText(_keyPath, encryptedKey);
            }
        }

        private byte[] GetDecryptedKey()
        {
            var encryptedKey = File.ReadAllText(_keyPath);
            var decryptedKeyHex = DecryptString(GetAESKey(), encryptedKey);
            return HexStringToByteArray(decryptedKeyHex);
        }

        public string ReadFileContent(string path)
        {
            var sr = new StreamReader(path);
            var content = sr.ReadToEnd();
            sr.Close();

            return DecryptString(GetDecryptedKey(), content);
        }

        public void WriteFileContent(string path, string content)
        {
            var sw = new StreamWriter(path);
            sw.WriteLine(EncryptString(GetDecryptedKey(), content));
            sw.Close();
        }

        private byte[] GetAESKey()
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(Talo.Settings.accessKey));
            }
        }

        private string EncryptString(byte[] key, string text)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(text);
                        sw.Flush();
                        cs.FlushFinalBlock();
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        private string DecryptString(byte[] key, string text)
        {
            var fullCipher = Convert.FromBase64String(text);

            using (var aes = Aes.Create())
            {
                aes.Key = key;

                var iv = new byte[aes.BlockSize / 8];
                Array.Copy(fullCipher, iv, iv.Length);

                var cipherBytes = new byte[fullCipher.Length - iv.Length];
                Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

                aes.IV = iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(cipherBytes))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private byte[] HexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
