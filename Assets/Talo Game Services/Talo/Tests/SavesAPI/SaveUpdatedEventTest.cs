using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TaloGameServices.Test
{
    internal class SaveUpdateEventMock
    {
        public int updatedCount = 0;
        public bool lastSuccess = false;
        public GameSave lastSave = null;

        public void OnUpdated(bool success, GameSave save)
        {
            updatedCount++;
            lastSuccess = success;
            lastSave = save;
        }
    }

    internal class SaveUpdatedEventTest
    {
        private SavesAPI _originalSavesApi;
        private SaveUpdateEventMock _mock;

        [OneTimeSetUp]
        public void SetUp()
        {
            var tm = new GameObject().AddComponent<TaloManager>();
            tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            tm.settings.autoConnectSocket = false;
            tm.settings.debounceTimerSeconds = 0.1f;

            Talo.CurrentAlias = new PlayerAlias() {
                player = new Player() {
                    id = "uuid"
                }
            };

            _originalSavesApi = Talo.Saves;
        }

        [SetUp]
        public void PerTestSetUp()
        {
            _mock = new SaveUpdateEventMock();
            RequestMock.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            Talo._saves = _originalSavesApi;
            RequestMock.Reset();
        }

        private SavesAPI BuildApiWithChosenSave(int saveId, string name)
        {
            var api = new SavesAPI();
            Talo._saves = api;
            api.Setup();

            var save = new GameSave { id = saveId, name = name };
            api.savesManager._allSaves.Add(save);
            api.savesManager.SetChosenSave(save, false);

            return api;
        }

        private static string PatchedSaveJson()
        {
            return JsonUtility.ToJson(new SavesPostResponse
            {
                save = new GameSave
                {
                    id = 1,
                    name = "Online Save",
                    content = new SaveContent(new Dictionary<string, SavedObject>()),
                    updatedAt = "2026-07-21T23:38:00.999Z"
                }
            });
        }

        [UnityTest]
        public IEnumerator LeadingCall_FiresOnSaveUpdated()
        {
            var api = BuildApiWithChosenSave(1, "Online Save");
            Talo.Saves.OnSaveUpdated += _mock.OnUpdated;

            var uri = new Uri(api.GetUri() + "/1");
            RequestMock.ReplyOnce(uri, "PATCH", PatchedSaveJson());

            _ = Talo.Saves.DebounceUpdate();

            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsTrue(_mock.lastSuccess);
            Assert.IsNotNull(_mock.lastSave);
            Assert.AreEqual(1, _mock.lastSave.id);

            yield return null;
        }

        [UnityTest]
        public IEnumerator LeadingAndTrailing_FireOnSaveUpdatedTwice()
        {
            var api = BuildApiWithChosenSave(1, "Online Save");
            Talo.Saves.OnSaveUpdated += _mock.OnUpdated;

            var uri = new Uri(api.GetUri() + "/1");
            RequestMock.Reply(uri, "PATCH", PatchedSaveJson());

            _ = Talo.Saves.DebounceUpdate();
            _ = Talo.Saves.DebounceUpdate();

            Assert.AreEqual(1, _mock.updatedCount);

            var result = Talo.Saves.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.Success, result);
            Assert.AreEqual(2, _mock.updatedCount);

            yield return null;
        }

        [UnityTest]
        public IEnumerator HttpError_FiresOnSaveUpdatedWithFalse()
        {
            BuildApiWithChosenSave(1, "Online Save");
            Talo.Saves.OnSaveUpdated += _mock.OnUpdated;

            // no mock for update means the leading call's Debounce() fails
            _ = Talo.Saves.DebounceUpdate();

            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsFalse(_mock.lastSuccess);
            Assert.IsNull(_mock.lastSave);

            yield return null;
        }

        [UnityTest]
        public IEnumerator TrailingCall_FailsOnHttpError_ReturnsFlushResultFailure()
        {
            var api = BuildApiWithChosenSave(1, "Online Save");
            Talo.Saves.OnSaveUpdated += _mock.OnUpdated;

            var uri = new Uri(api.GetUri() + "/1");
            RequestMock.ReplyOnce(uri, "PATCH", PatchedSaveJson());

            _ = Talo.Saves.DebounceUpdate();
            _ = Talo.Saves.DebounceUpdate();

            Assert.AreEqual(1, _mock.updatedCount);
            Assert.IsTrue(_mock.lastSuccess);

            // trailing call has no mock so flush returns Failure (not throw)
            var flushResult = Talo.Saves.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.Failure, flushResult);

            yield return null;
        }

        [UnityTest]
        public IEnumerator FlushUpdates_NoPending_ReturnsNothingPending()
        {
            BuildApiWithChosenSave(1, "Online Save");
            var result = Talo.Saves.FlushUpdates().GetAwaiter().GetResult();
            Assert.AreEqual(DebouncedAPIBase.FlushResult.NothingPending, result);
            yield return null;
        }
    }
}
