using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TaloGameServices.Test
{
    internal class RequestHeadersTest
    {
        private const string PlayerHeader = "X-Talo-Player";
        private const string AliasHeader = "X-Talo-Alias";

        private PlayerAlias _originalAlias;

        [OneTimeSetUp]
        public void SetUp()
        {
            var tm = new GameObject().AddComponent<TaloManager>();
            tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            tm.settings.autoConnectSocket = false;
        }

        [SetUp]
        public void PerTestSetUp()
        {
            _originalAlias = Talo.CurrentAlias;
            Talo.CurrentAlias = new PlayerAlias { id = 1, player = new Player { id = "player-1" } };
            Talo.ClaimKeys.ClearCurrentKey();
            ClaimKeysTestFileHandler.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            Talo.CurrentAlias = _originalAlias;
            Talo.ClaimKeys.ClearCurrentKey();
            ClaimKeysTestFileHandler.Reset();
        }

        [Test]
        public void ClaimKeyHeaderMatchesTheWireName()
        {
            // the suite reads the constant everywhere else, so this is the one place that pins it
            Assert.AreEqual("X-Talo-Claim-Key", ClaimKeyManager.Header);
        }

        [Test]
        public void SendsTheClaimKeyOfTheCurrentPlayer()
        {
            Talo.ClaimKeys.SaveKey("player-1", "caller-key");

            var headers = new BaseAPI("v1/players").BuildHeaders();

            Assert.AreEqual("player-1", ValueOf(headers, PlayerHeader));
            Assert.AreEqual("caller-key", ValueOf(headers, ClaimKeyManager.Header));
        }

        [Test]
        public void DoesNotSendTheClaimKeyOfAnotherPlayer()
        {
            Talo.ClaimKeys.SaveKey("player-2", "other-key");

            var headers = new BaseAPI("v1/players").BuildHeaders();

            Assert.IsEmpty(ValueOf(headers, ClaimKeyManager.Header));
        }

        [Test]
        public void SendsNoIdentityWhenTheCallerAsksForNone()
        {
            Talo.ClaimKeys.SaveKey("player-1", "caller-key");

            var headers = new BaseAPI("v1/players").BuildHeaders(claimKey: "caller-key", sendIdentity: false);

            Assert.IsEmpty(ValueOf(headers, PlayerHeader));
            Assert.IsEmpty(ValueOf(headers, AliasHeader));
            Assert.AreEqual("caller-key", ValueOf(headers, ClaimKeyManager.Header));
        }

        [Test]
        public void SendsTheSignatureBeforeThePlayerIsResolved()
        {
            Talo.CurrentAlias = null;
            Talo.Settings.verificationEnabled = true;
            Talo.Settings.verificationKeyVersion = "1";
            Talo.Settings.verificationKeyValue = "key";

            try
            {
                var headers = new BaseAPI("v1/players").BuildHeaders("", sendIdentity: false);

                Assert.IsNotEmpty(ValueOf(headers, "X-Talo-Signature"));
            }
            finally
            {
                Talo.Settings.verificationEnabled = false;
                Talo.Settings.verificationKeyVersion = "";
                Talo.Settings.verificationKeyValue = "";
            }
        }

        private static string ValueOf(List<HttpHeader> headers, string key)
        {
            return headers.Find((header) => header.key == key)?.value ?? "";
        }
    }
}
