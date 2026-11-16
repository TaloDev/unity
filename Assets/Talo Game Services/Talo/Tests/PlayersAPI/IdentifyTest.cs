using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TaloGameServices.Test
{
    internal class IdentifyTest
    {
        private PlayerAlias _originalAlias;
        private readonly List<IdentifyException> _identifyFailures = new();

        private static Uri IdentifyUri => new($"{Talo.Settings.apiUrl}/v1/players/identify");

        [OneTimeSetUp]
        public void SetUp()
        {
            var tm = new GameObject().AddComponent<TaloManager>();
            tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            tm.settings.autoConnectSocket = false;
            tm.settings.logRequests = false;
            tm.settings.logResponses = false;
        }

        [SetUp]
        public void PerTestSetUp()
        {
            _originalAlias = Talo.CurrentAlias;
            Talo.CurrentAlias = null;
            Talo.ClaimKeys.ClearCurrentKey();
            ClaimKeysTestFileHandler.Reset();
            RequestMock.Reset();

            Talo.Players.OnIdentificationFailed += OnIdentificationFailed;
        }

        [TearDown]
        public void TearDown()
        {
            Talo.Players.OnIdentificationFailed -= OnIdentificationFailed;
            Talo.CurrentAlias = _originalAlias;
            Talo.ClaimKeys.ClearCurrentKey();
            ClaimKeysTestFileHandler.Reset();
            RequestMock.Reset();
            _identifyFailures.Clear();
        }

        [Test]
        public void SendsPostWithTheClaimKey()
        {
            RequestMock.ReplyOnce(IdentifyUri, "POST", IdentifySuccess());

            var alias = Talo.Players.Identify("username", "bob", "caller-key").GetAwaiter().GetResult();

            var request = RequestMock.Requests[0];
            Assert.AreEqual("POST", request.method);
            Assert.AreEqual(IdentifyUri, request.uri);
            Assert.AreEqual("caller-key", request.GetHeader(ClaimKeyManager.Header));
            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetCurrentKey(Talo.CurrentPlayer));
            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetPlayerKey(alias.player.id));
        }

        [Test]
        public void SendsNoIdentityOfThePreviousPlayer()
        {
            Talo.CurrentAlias = new PlayerAlias { id = 1, player = new Player { id = "player-9" } };
            Talo.ClaimKeys.SaveKey("player-9", "other-key");
            RequestMock.ReplyOnce(IdentifyUri, "POST", IdentifySuccess());

            Talo.Players.Identify("username", "bob").GetAwaiter().GetResult();

            var request = RequestMock.Requests[0];
            Assert.IsEmpty(request.GetHeader("X-Talo-Player"));
            Assert.IsEmpty(request.GetHeader("X-Talo-Alias"));
            Assert.IsEmpty(request.GetHeader(ClaimKeyManager.Header));
        }

        [Test]
        public void WithoutAClaimKeyStoresNothing()
        {
            RequestMock.ReplyOnce(IdentifyUri, "POST", IdentifySuccess());

            var alias = Talo.Players.Identify("username", "bob").GetAwaiter().GetResult();

            Assert.IsEmpty(RequestMock.Requests[0].GetHeader(ClaimKeyManager.Header));
            Assert.IsEmpty(Talo.ClaimKeys.GetCurrentKey(Talo.CurrentPlayer));
            Assert.IsEmpty(Talo.ClaimKeys.GetPlayerKey(alias.player.id));
        }

        [Test]
        public void ReusesTheStoredClaimKeyAfterARelaunch()
        {
            RequestMock.ReplyOnce(IdentifyUri, "POST", IdentifySuccess());
            Talo.Players.Identify("username", "bob", "caller-key").GetAwaiter().GetResult();

            // a relaunch starts with no claim key in memory
            Talo.CurrentAlias = null;
            Talo.ClaimKeys.ClearCurrentKey();

            RequestMock.ReplyOnce(IdentifyUri, "POST", IdentifySuccess());
            Talo.Players.Identify("username", "bob").GetAwaiter().GetResult();

            Assert.AreEqual("caller-key", RequestMock.Requests[1].GetHeader(ClaimKeyManager.Header));
            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetCurrentKey(Talo.CurrentPlayer));
        }

        [Test]
        public void StoresTheResolvedIdentifierOfAnIntegration()
        {
            // steam answers with the resolved steam id, never with the ticket that was sent
            RequestMock.ReplyOnce(IdentifyUri, "POST", IdentifySuccess("76561198000000000", "steam"));

            Talo.Players.Identify("steam", "ticket-1", "caller-key").GetAwaiter().GetResult();

            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetIdentifyKey("steam", "76561198000000000"));
            Assert.IsEmpty(Talo.ClaimKeys.GetIdentifyKey("steam", "ticket-1"));
        }

        [Test]
        public void RejectedClaimKey_LeavesThePlayerAndTheStoreAlone()
        {
            RequestMock.ReplyOnce(IdentifyUri, "POST", IdentifySuccess());
            Talo.Players.Identify("username", "bob", "old-key").GetAwaiter().GetResult();
            var player = Talo.CurrentPlayer;

            RequestMock.ReplyOnce(IdentifyUri, "POST", InvalidClaimKey(), 401);

            Assert.Throws<RequestException>(() =>
                Talo.Players.Identify("username", "bob", "wrong-key").GetAwaiter().GetResult()
            );

            // any other identify failure clears the identity, this one must not
            Assert.AreSame(player, Talo.CurrentPlayer);
            Assert.AreEqual(IdentifyErrorCode.INVALID_CLAIM_KEY, _identifyFailures[0].ErrorCode);
            Assert.AreEqual(2, RequestMock.Requests.Count);
            Assert.AreEqual("old-key", Talo.ClaimKeys.GetCurrentKey(Talo.CurrentPlayer));
            Assert.AreEqual("old-key", Talo.ClaimKeys.GetPlayerKey(player.id));
            Assert.AreEqual("old-key", Talo.ClaimKeys.GetIdentifyKey("username", "bob"));
        }

        [Test]
        public void StoresTheGameCenterIdentifierUnescaped()
        {
            var signature = new byte[] { 1, 2, 3 };
            var salt = new byte[] { 4, 5, 6 };
            var identifier = JsonUtility.ToJson(new PlayersGameCenterIdentifier
            {
                publicKeyUrl = "https://apple.com/cert",
                signature = Convert.ToBase64String(signature),
                salt = Convert.ToBase64String(salt),
                timestamp = 1234,
                playerId = "G:1234",
                bundleId = Application.identifier
            });
            RequestMock.ReplyOnce(IdentifyUri, "POST", IdentifySuccess(identifier, "game_center"));

            Talo.Players.IdentifyGameCenter("https://apple.com/cert", signature, salt, 1234, "G:1234", "caller-key")
                .GetAwaiter().GetResult();

            var sent = JsonUtility.FromJson<PlayersIdentifyRequest>(RequestMock.Requests[0].content);
            Assert.AreEqual(identifier, sent.identifier);
            Assert.AreEqual("caller-key", Talo.ClaimKeys.GetIdentifyKey("game_center", identifier));
        }

        [Test]
        public void Offline_SendsNothing()
        {
            RequestMock.Offline = true;
            try
            {
                Assert.Throws<Exception>(() =>
                    Talo.Players.Identify("username", "bob", "caller-key").GetAwaiter().GetResult()
                );
            }
            finally
            {
                RequestMock.Offline = false;
            }

            Assert.IsEmpty(RequestMock.Requests);
        }

        private void OnIdentificationFailed(IdentifyException exception)
        {
            _identifyFailures.Add(exception);
        }

        private static string IdentifySuccess(string identifier = "bob", string service = "username")
        {
            return JsonUtility.ToJson(new PlayersIdentifyResponse
            {
                alias = new PlayerAlias
                {
                    id = 1,
                    service = service,
                    identifier = identifier,
                    player = new Player { id = "player-1" }
                }
            });
        }

        private static string InvalidClaimKey()
        {
            return JsonUtility.ToJson(new ErrorResponse
            {
                message = "Missing or invalid claim key",
                errorCode = "INVALID_CLAIM_KEY"
            });
        }
    }
}
