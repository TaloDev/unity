using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TaloGameServices.Test
{
    internal class QueuedRequestHeadersTest
    {
        private const string PlayerId = "player-id";
        private const int AliasId = 42;
        private const string QueueUrl = "https://api.trytalo.com/v1/events";
        private const string UnreachableApiUrl = "http://127.0.0.1:1";
        private const string Signature = "1|signature";
        private const long FirstQueuedAt = 1000;
        private const long ReplayedAt = 2000;

        private string _originalApiUrl;

        [OneTimeSetUp]
        public void SetUp()
        {
            var tm = new GameObject().AddComponent<TaloManager>();
            tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            tm.settings.autoConnectSocket = false;
            tm.settings.logRequests = false;
            tm.settings.logResponses = false;

            Talo.CurrentAlias = new PlayerAlias
            {
                id = AliasId,
                player = new Player { id = PlayerId }
            };

            _originalApiUrl = Talo.Settings.apiUrl;
            Talo.Continuity.ClearRequests();
        }

        [TearDown]
        public void TearDown()
        {
            TestModeFlag.IsEnabled = true;
            Talo.Settings.apiUrl = _originalApiUrl;
            Talo._healthCheck = new HealthCheckAPI();
            Talo.Continuity.ClearRequests();
        }

        [UnityTest]
        public IEnumerator FailedRequest_QueuesTheIdentityHeadersItWasSentWith()
        {
            Talo.Settings.apiUrl = UnreachableApiUrl;
            Talo._healthCheck = new HealthCheckAPI();
            TestModeFlag.IsEnabled = false;

            var track = new StatsAPI().Track("score");
            while (!track.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(track.IsFaulted);
            Assert.AreEqual(1, Talo.Continuity.Requests.Count);

            var request = Talo.Continuity.Requests[0];
            Assert.AreEqual(PlayerId, GetHeader(request, "X-Talo-Player"));
            Assert.AreEqual(AliasId.ToString(), GetHeader(request, "X-Talo-Alias"));
        }

        [Test]
        public void ReplayOnlyHeadersAreNotQueued()
        {
            var headers = new List<HttpHeader>
            {
                new("X-Talo-Player", PlayerId),
                new(ContinuityManager.AuthorizationHeader, "Bearer access-key"),
                new(ContinuityManager.SignatureHeader, Signature),
                new(ContinuityManager.ContinuityTimestampHeader, FirstQueuedAt.ToString())
            };

            Talo.Continuity.PushRequest(new System.Uri(QueueUrl), "POST", "{}", headers, ReplayedAt);

            // ProcessRequests writes these three headers itself
            // The timestamp it uses is the original timestamp from when the request was queued
            var request = Talo.Continuity.Requests[0];
            Assert.IsNull(GetHeader(request, ContinuityManager.AuthorizationHeader));
            Assert.IsNull(GetHeader(request, ContinuityManager.SignatureHeader));
            Assert.IsNull(GetHeader(request, ContinuityManager.ContinuityTimestampHeader));
            Assert.AreEqual(PlayerId, GetHeader(request, "X-Talo-Player"));
            Assert.AreEqual(FirstQueuedAt, request.timestamp);
        }

        private static string GetHeader(Request request, string key)
        {
            return request.headers.Find((h) => h.key == key)?.value;
        }
    }
}
