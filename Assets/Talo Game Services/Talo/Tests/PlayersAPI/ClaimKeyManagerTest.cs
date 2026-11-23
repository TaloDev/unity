using NUnit.Framework;
using UnityEngine;

namespace TaloGameServices.Test
{
    internal class ClaimKeyManagerTest
    {
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
            Talo.ClaimKeys.ClearCurrentKey();
            ClaimKeysTestFileHandler.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            Talo.ClaimKeys.ClearCurrentKey();
            ClaimKeysTestFileHandler.Reset();
        }

        [Test]
        public void UnknownPlayer_HasNoKey()
        {
            Assert.IsEmpty(Talo.ClaimKeys.GetPlayerKey("player-1"));
        }

        [Test]
        public void UnknownIdentifyRequest_HasNoKey()
        {
            Assert.IsEmpty(Talo.ClaimKeys.GetIdentifyKey("username", "bob"));
        }

        [Test]
        public void Keys_AreStoredPerPlayer()
        {
            Talo.ClaimKeys.SaveKey("player-1", "first-key", "username", "bob");
            Talo.ClaimKeys.SaveKey("player-2", "second-key", "username", "alice");

            Assert.AreEqual("first-key", Talo.ClaimKeys.GetPlayerKey("player-1"));
            Assert.AreEqual("second-key", Talo.ClaimKeys.GetPlayerKey("player-2"));
            Assert.AreEqual("first-key", Talo.ClaimKeys.GetIdentifyKey("username", "bob"));
            Assert.AreEqual("second-key", Talo.ClaimKeys.GetIdentifyKey("username", "alice"));
        }

        [Test]
        public void NewStoreInstance_ReadsTheSavedKeysBack()
        {
            Talo.ClaimKeys.SaveKey("player-1", "caller-key", "username", "bob");

            var store = new ClaimKeyManager();

            Assert.AreEqual("caller-key", store.GetPlayerKey("player-1"));
            Assert.AreEqual("caller-key", store.GetIdentifyKey("username", "bob"));
        }

        [Test]
        public void FindsTheKeyWhateverTheServiceCasingAndSpacing()
        {
            Talo.ClaimKeys.SaveKey("player-1", "caller-key", "username", "bob");

            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetIdentifyKey(" Username ", " bob "));
        }

        [Test]
        public void SavingWithoutAKey_IsIgnored()
        {
            Talo.ClaimKeys.SaveKey("player-1", "", "username", "bob");

            Assert.IsEmpty(Talo.ClaimKeys.GetPlayerKey("player-1"));
            Assert.IsEmpty(Talo.ClaimKeys.GetIdentifyKey("username", "bob"));
        }

        [Test]
        public void SavingWithoutAnIdentity_RecordsOnlyThePlayer()
        {
            Talo.ClaimKeys.SaveKey("player-1", "caller-key");

            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetPlayerKey("player-1"));
            Assert.IsEmpty(Talo.ClaimKeys.GetIdentifyKey("username", "bob"));
        }

        [Test]
        public void SavingAgain_ReplacesTheKeyOfAPlayer()
        {
            Talo.ClaimKeys.SaveKey("player-1", "first-key");
            Talo.ClaimKeys.SaveKey("player-1", "second-key");

            Assert.AreEqual("second-key", Talo.ClaimKeys.GetPlayerKey("player-1"));
        }

        [Test]
        public void ForgettingAPlayer_RemovesTheKey()
        {
            Talo.ClaimKeys.SaveKey("player-1", "caller-key");

            Talo.ClaimKeys.ForgetPlayer("player-1");

            Assert.IsEmpty(Talo.ClaimKeys.GetPlayerKey("player-1"));
        }

        [Test]
        public void CurrentKey_IsEmptyWithoutAPlayer()
        {
            Assert.IsEmpty(Talo.ClaimKeys.GetCurrentKey(null));
        }
    }
}
