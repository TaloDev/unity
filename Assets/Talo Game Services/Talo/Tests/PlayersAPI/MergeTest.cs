using System;
using NUnit.Framework;
using UnityEngine;

namespace TaloGameServices.Test
{
    internal class MergeTest
    {
        private PlayerAlias _originalAlias;

        private static Uri MergeUri => new($"{Talo.Settings.apiUrl}/v1/players/merge");

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
            Talo.CurrentAlias = null;
            Talo.ClaimKeys.ClearCurrentKey();
            ClaimKeysTestFileHandler.Reset();
            RequestMock.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            Talo.CurrentAlias = _originalAlias;
            Talo.ClaimKeys.ClearCurrentKey();
            ClaimKeysTestFileHandler.Reset();
            RequestMock.Reset();
        }

        [Test]
        public void SendsTheStoredClaimKeyOfTheSecondPlayer()
        {
            Talo.CurrentAlias = MakeAlias("player-1");
            Talo.ClaimKeys.SaveKey("player-2", "alice-key");
            RequestMock.ReplyOnce(MergeUri, "POST", MergeSuccess());

            Talo.Players.Merge("player-1", "player-2").GetAwaiter().GetResult();

            Assert.AreEqual("alice-key", SentMergeBody().player2ClaimKey);
        }

        [Test]
        public void PrefersTheSuppliedClaimKey()
        {
            Talo.CurrentAlias = MakeAlias("player-1");
            Talo.ClaimKeys.SaveKey("player-2", "stored-key");
            RequestMock.ReplyOnce(MergeUri, "POST", MergeSuccess());

            Talo.Players.Merge("player-1", "player-2", new MergeOptions { player2ClaimKey = "game-key" })
                .GetAwaiter().GetResult();

            Assert.AreEqual("game-key", SentMergeBody().player2ClaimKey);
        }

        [Test]
        public void OmitsTheClaimKeyWhenTheSecondPlayerHasNone()
        {
            Talo.CurrentAlias = MakeAlias("player-1");
            RequestMock.ReplyOnce(MergeUri, "POST", MergeSuccess());

            Talo.Players.Merge("player-1", "player-2").GetAwaiter().GetResult();

            StringAssert.DoesNotContain("player2ClaimKey", RequestMock.Requests[0].content);
        }

        [Test]
        public void TakesTheClaimKeyOfTheMergedPlayer()
        {
            Talo.CurrentAlias = MakeAlias("player-1");
            Talo.ClaimKeys.SaveKey("player-2", "alice-key");
            RequestMock.ReplyOnce(MergeUri, "POST", MergeSuccess(MakeAlias("player-1", "username", "alice")));

            Talo.Players.Merge("player-1", "player-2").GetAwaiter().GetResult();

            Assert.AreEqual("alice-key", Talo.ClaimKeys.GetPlayerKey("player-1"));
            Assert.AreEqual("alice-key", Talo.ClaimKeys.GetCurrentKey(Talo.CurrentPlayer));
            Assert.AreEqual("alice-key", Talo.ClaimKeys.GetIdentifyKey("username", "alice"));
            Assert.IsEmpty(Talo.ClaimKeys.GetPlayerKey("player-2"));
        }

        [Test]
        public void KeepsTheClaimKeyOfTheSurvivingPlayer()
        {
            Talo.CurrentAlias = MakeAlias("player-1");
            Talo.ClaimKeys.SaveKey("player-1", "bob-key");
            Talo.ClaimKeys.SaveKey("player-2", "alice-key");
            RequestMock.ReplyOnce(MergeUri, "POST", MergeSuccess(MakeAlias("player-1", "username", "alice")));

            Talo.Players.Merge("player-1", "player-2").GetAwaiter().GetResult();

            Assert.AreEqual("bob-key", Talo.ClaimKeys.GetPlayerKey("player-1"));
            Assert.AreEqual("bob-key", Talo.ClaimKeys.GetIdentifyKey("username", "alice"));
        }

        private static PlayerAlias MakeAlias(string playerId, string service = "username", string identifier = "bob")
        {
            return new PlayerAlias
            {
                id = 1,
                service = service,
                identifier = identifier,
                player = new Player { id = playerId }
            };
        }

        private static string MergeSuccess(PlayerAlias alias = null)
        {
            var aliases = alias == null ? Array.Empty<PlayerAlias>() : new[] { alias };

            return JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "player-1", aliases = aliases }
            });
        }

        private static PlayersMergeRequest SentMergeBody()
        {
            return JsonUtility.FromJson<PlayersMergeRequest>(RequestMock.Requests[0].content);
        }
    }
}
