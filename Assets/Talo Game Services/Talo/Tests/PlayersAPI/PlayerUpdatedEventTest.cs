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
            tm.settings.debounceTimerSeconds = 0f;

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
        public IEnumerator TrailingCall_FiresOnPlayerUpdated()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.ReplyOnce(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" }
            }));

            Talo.CurrentPlayer.SetProp("k1", "v1-updated");

            yield return null;

            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsTrue(_mock.lastSuccess);
        }

        [UnityTest]
        public IEnumerator TrailingCall_FiresOnceForMultipleCalls()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.Reply(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" }
            }));

            Talo.CurrentPlayer.SetProp("k1", "v1-updated");
            Talo.CurrentPlayer.SetProp("k2", "v2");

            yield return null;

            Assert.AreEqual(1, _mock.updatedCount);

            var result = Talo.Players.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.NothingPending, result);
            Assert.AreEqual(1, _mock.updatedCount);
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

            var task = Talo.CurrentPlayer.SetProp("k1", "v1-updated");

            yield return null;

            var result = task.GetAwaiter().GetResult();

            Assert.AreEqual(1, result.RejectedProps.Length);
            Assert.AreEqual("k1", result.RejectedProps[0].key);
            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsTrue(_mock.lastSuccess);
        }

        [UnityTest]
        public IEnumerator TrailingCall_HttpError_FiresOnPlayerUpdatedWithFalse()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            // no mock registered — RequestMock.HandleCall throws, simulating an HTTP error
            Talo.CurrentPlayer.SetProp("k1", "v1-updated");

            yield return null;

            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsFalse(_mock.lastSuccess);
        }

        [UnityTest]
        public IEnumerator TrailingCall_HttpError_FlushReturnsFailure()
        {
            Talo.Players.OnPlayerUpdated += _mock.OnUpdated;

            // no mock registered — RequestMock.HandleCall throws, simulating an HTTP error
            Talo.CurrentPlayer.SetProp("k1", "v1-updated");

            var flushResult = Talo.Players.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.Failure, flushResult);
            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsFalse(_mock.lastSuccess);

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

            Assert.AreEqual(0, _mock.updatedCount);

            var result = Talo.Players.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.Success, result);
            Assert.AreEqual(1, _mock.updatedCount);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetProp_ReturnsResult()
        {
            var uri = new Uri($"{Talo.Settings.apiUrl}/v1/players/uuid");
            RequestMock.ReplyOnce(uri, "PATCH", JsonUtility.ToJson(new PlayersUpdateResponse
            {
                player = new Player { id = "uuid" }
            }));

            var task = Talo.CurrentPlayer.SetProp("k1", "v1-updated");

            yield return null;

            var result = task.GetAwaiter().GetResult();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.RejectedProps.Length);
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
