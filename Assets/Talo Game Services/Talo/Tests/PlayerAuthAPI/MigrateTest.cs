using System;
using NUnit.Framework;
using UnityEngine;

namespace TaloGameServices.Test
{
    internal class MigrateTest
    {
        private const string SteamIdentifier = "76561198000000000";
        private const string SteamTicket = "ticket-1";

        private PlayerAlias _originalAlias;

        private static Uri MigrateUri => new($"{Talo.Settings.apiUrl}/v1/players/auth/migrate");

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
            Talo.CurrentAlias = new PlayerAlias { id = 1, service = "talo", identifier = "bob", player = new Player { id = "player-1" } };
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
        public void SendsTheClaimKeyAndSavesItForTheResolvedIdentity()
        {
            RequestMock.ReplyOnce(MigrateUri, "POST", MigrateSuccess());

            // steam answers with the resolved id, never with the ticket that was sent
            Talo.PlayerAuth.MigrateAccount("password", "Steam", SteamTicket, "caller-key")
                .GetAwaiter().GetResult();

            Assert.AreEqual("caller-key", RequestMock.Requests[0].GetHeader(ClaimKeyManager.Header));
            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetPlayerKey("player-1"));
            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetIdentifyKey("steam", SteamIdentifier));
            Assert.IsEmpty(Talo.ClaimKeys.GetIdentifyKey("steam", SteamTicket));
            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetCurrentKey(Talo.CurrentPlayer));
        }

        private static string MigrateSuccess()
        {
            return JsonUtility.ToJson(new PlayerAuthMigrateAccountResponse
            {
                alias = new PlayerAlias
                {
                    id = 1,
                    service = "steam",
                    identifier = SteamIdentifier,
                    player = new Player { id = "player-1" }
                }
            });
        }
    }
}
