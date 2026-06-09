using System;
using System.Text;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;

namespace TaloGameServices.Test
{
    internal class CreateRequestSignatureTest
    {
        private const string TestBody = "{\"foo\":\"bar\"}";
        private const string TestKeyVersion = "1";
        private const string TestKeyValue = "test-secret-key";

        private TaloManager _tm;

        [OneTimeSetUp]
        public void Setup()
        {
            _tm = new GameObject().AddComponent<TaloManager>();
            _tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            _tm.settings.autoConnectSocket = false;
            _tm.settings.verificationEnabled = true;
            _tm.settings.verificationKeyVersion = TestKeyVersion;
            _tm.settings.verificationKeyValue = TestKeyValue;
        }

        [Test]
        public void CreateRequestSignature_ShouldStartWithKeyVersion()
        {
            var token = CryptoManager.CreateRequestSignature(TestBody);
            Assert.That(token.StartsWith(TestKeyVersion + "|"));
        }

        [Test]
        public void CreateRequestSignature_ShouldHaveCorrectFormat()
        {
            var token = CryptoManager.CreateRequestSignature(TestBody);
            var parts = token.Split('|');
            Assert.That(parts.Length, Is.EqualTo(2), "Token should have one | separator");
            Assert.That(parts[0], Is.EqualTo(TestKeyVersion));

            var headerAndSig = parts[1].Split('.');
            Assert.That(headerAndSig.Length, Is.EqualTo(2), "Header and signature should be separated by .");

            var headerB64 = headerAndSig[0];
            var signatureB64 = headerAndSig[1];
            Assert.That(headerB64, Is.Not.Empty);
            Assert.That(signatureB64, Is.Not.Empty);
        }

        [Test]
        public void CreateRequestSignature_HeaderShouldDecodeToValidPayload()
        {
            var token = CryptoManager.CreateRequestSignature(TestBody);
            var headerAndSig = token.Split('|')[1].Split('.');
            var headerB64 = headerAndSig[0];

            var headerJson = Encoding.UTF8.GetString(Convert.FromBase64String(headerB64));

            Assert.That(headerJson, Does.Contain("\"rid\""), "Header should contain rid field");
            Assert.That(headerJson, Does.Contain("\"payload\""), "Header should contain payload field");
            Assert.That(headerJson, Does.Contain("\"timestamp\""), "Header should contain timestamp field");
        }

        [Test]
        public void CreateRequestSignature_PayloadShouldBeBodySha256Hex()
        {
            var token = CryptoManager.CreateRequestSignature(TestBody);
            var headerAndSig = token.Split('|')[1].Split('.');
            var headerB64 = headerAndSig[0];
            var headerJson = Encoding.UTF8.GetString(Convert.FromBase64String(headerB64));

            using var sha256 = SHA256.Create();
            var expectedHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(TestBody));
            var expectedPayload = BitConverter.ToString(expectedHashBytes).Replace("-", "").ToLower();

            Assert.That(headerJson, Does.Contain("\"payload\":\"" + expectedPayload + "\""));
        }

        [Test]
        public void CreateRequestSignature_SignatureShouldVerifyWithKeyValue()
        {
            var token = CryptoManager.CreateRequestSignature(TestBody);
            var headerAndSig = token.Split('|')[1].Split('.');
            var headerB64 = headerAndSig[0];
            var signatureB64 = headerAndSig[1];

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(TestKeyValue));
            var expectedSignatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(headerB64));
            var expectedSignatureB64 = Convert.ToBase64String(expectedSignatureBytes);

            Assert.That(signatureB64, Is.EqualTo(expectedSignatureB64),
                "Signature should match HMAC-SHA256 of headerB64 using the key value");
        }

        [Test]
        public void CreateRequestSignature_DifferentBodiesShouldProduceDifferentTokens()
        {
            var token1 = CryptoManager.CreateRequestSignature("body1");
            var token2 = CryptoManager.CreateRequestSignature("body2");

            Assert.That(token1, Is.Not.EqualTo(token2),
                "Different request bodies should produce different tokens");
        }

        [Test]
        public void CreateRequestSignature_SameBodyShouldProduceDifferentTokensDueToRid()
        {
            var token1 = CryptoManager.CreateRequestSignature(TestBody);
            var token2 = CryptoManager.CreateRequestSignature(TestBody);

            Assert.That(token1, Is.Not.EqualTo(token2),
                "Same body should produce different tokens due to unique rid");
        }

        [Test]
        public void CreateRequestSignature_WithEmptyBody_ShouldProduceValidSignature()
        {
            var token = CryptoManager.CreateRequestSignature("");
            Assert.That(token, Is.Not.Null.And.Not.Empty);
            Assert.That(token.StartsWith(TestKeyVersion + "|"));

            var headerAndSig = token.Split('|')[1].Split('.');
            Assert.That(headerAndSig.Length, Is.EqualTo(2));
            Assert.That(headerAndSig[0], Is.Not.Empty);
            Assert.That(headerAndSig[1], Is.Not.Empty);

            var headerJson = Encoding.UTF8.GetString(Convert.FromBase64String(headerAndSig[0]));
            Assert.That(headerJson, Does.Contain("\"rid\""));
            Assert.That(headerJson, Does.Contain("\"payload\""));
            Assert.That(headerJson, Does.Contain("\"timestamp\""));

            using var sha256 = SHA256.Create();
            var expectedHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(""));
            var expectedPayload = BitConverter.ToString(expectedHashBytes).Replace("-", "").ToLower();
            Assert.That(headerJson, Does.Contain("\"payload\":\"" + expectedPayload + "\""));
        }

        [Test]
        public void CreateRequestSignature_SameEmptyBody_ShouldProduceDifferentTokensDueToRid()
        {
            var token1 = CryptoManager.CreateRequestSignature("");
            var token2 = CryptoManager.CreateRequestSignature("");

            Assert.That(token1, Is.Not.EqualTo(token2),
                "Same empty body should produce different tokens due to unique rid");
        }

        [Test]
        public void CreateRequestSignature_WithVerificationDisabled_ShouldStillWork()
        {
            _tm.settings.verificationEnabled = false;

            var token = CryptoManager.CreateRequestSignature(TestBody);
            Assert.That(token, Is.Not.Null.And.Not.Empty);
            Assert.That(token.StartsWith(TestKeyVersion + "|"));

            _tm.settings.verificationEnabled = true;
        }
    }
}
