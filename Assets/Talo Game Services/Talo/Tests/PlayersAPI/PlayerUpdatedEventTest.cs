using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TaloGameServices.Test
{
    internal class PlayerUpdateEventMock
    {
        public int updatedCount = 0;
        public bool lastSuccess = false;

        public void OnUpdated(bool success)
        {
            updatedCount++;
            lastSuccess = success;
        }
    }

    internal class PlayerUpdatedEventTest
    {
        private PlayersAPI _originalPlayersApi;
        private PlayerUpdateEventMock _mock;

        [OneTimeSetUp]
        public void SetUp()
        {
            var tm = new GameObject().AddComponent<TaloManager>();
            tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            tm.settings.autoConnectSocket = false;
            tm.settings.debounceTimerSeconds = 0.1f;

            Talo.CurrentAlias = new PlayerAlias() {
                player = new Player() {
                    id = "uuid",
                    props = new[] { new Prop(("k1", "v1")) }
                }
            };

            _originalPlayersApi = Talo.Players;
        }

        [SetUp]
        public void PerTestSetUp()
        {
            _mock = new PlayerUpdateEventMock();
            Talo._players = new PlayersAPI();
            RequestMock.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            Talo._players = _originalPlayersApi;
            RequestMock.Reset();
        }

        [UnityTest]
        public IEnumerator LeadingCall_FiresOnPlayerUpdated()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.ReplyOnce(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" }
            }));

            Talo.CurrentPlayer.SetProp("k1", "v1-updated");

            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsTrue(_mock.lastSuccess);

            yield return null;
        }

        [UnityTest]
        public IEnumerator LeadingAndTrailing_FireOnPlayerUpdatedTwice()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.Reply(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" }
            }));

            Talo.CurrentPlayer.SetProp("k1", "v1-updated");
            Talo.CurrentPlayer.SetProp("k2", "v2");

            Assert.AreEqual(1, _mock.updatedCount);

            var result = Talo.Players.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.Success, result);
            Assert.AreEqual(2, _mock.updatedCount);

            yield return null;
        }

        [UnityTest]
        public IEnumerator PropRejection_OnPlayerUpdatedFiresWithRejectedProps()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.ReplyOnce(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" },
                rejectedProps = new[] { new RejectedProp { key = "k1", error = "PROP_VALUE_TOO_LONG", message = "too long" } }
            }));

            var result = Talo.CurrentPlayer.SetProp("k1", "v1-updated").GetAwaiter().GetResult();

            Assert.AreEqual(1, result.RejectedProps.Length);
            Assert.AreEqual("k1", result.RejectedProps[0].key);
            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsTrue(_mock.lastSuccess);

            yield return null;
        }

        [UnityTest]
        public IEnumerator HttpError_FiresOnPlayerUpdatedWithFalse()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            // no mock for update means the leading call's Debounce() fails

            Talo.CurrentPlayer.SetProp("k1", "v1-updated");

            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsFalse(_mock.lastSuccess);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TrailingCall_FailsOnHttpError_ReturnsFlushResultFailure()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.ReplyOnce(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" }
            }));

            Talo.CurrentPlayer.SetProp("k1", "v1-updated");
            Talo.CurrentPlayer.SetProp("k2", "v2");

            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsTrue(_mock.lastSuccess);

            // trailing call has no mock so flush returns Failure (not throw)
            var flushResult = Talo.Players.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.Failure, flushResult);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FlushUpdates_NoPending_ReturnsNothingPending()
        {
            var result = Talo.Players.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.NothingPending, result);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FlushUpdates_WithTrailingQueued_ReturnsSuccessAndFiresEvent()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.Reply(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" }
            }));

            Talo.CurrentPlayer.SetProp("k1", "v1-updated");
            Talo.CurrentPlayer.SetProp("k2", "v2");

            Assert.AreEqual(1, _mock.updatedCount);

            var result = Talo.Players.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.Success, result);
            Assert.AreEqual(2, _mock.updatedCount);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetProp_ReturnsResultInline()
        {
            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.ReplyOnce(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" }
            }));

            var result = Talo.CurrentPlayer.SetProp("k1", "v1-updated").GetAwaiter().GetResult();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.RejectedProps.Length);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetProp_LocalOnly_ReturnsImmediateSuccess()
        {
            var result = Talo.CurrentPlayer.SetProp("k1", "v1-local", false).GetAwaiter().GetResult();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.RejectedProps.Length);

            yield return null;
        }
    }
}
