using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System;

namespace TaloGameServices.Test
{
    internal class DeleteSaveTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var tm = new GameObject().AddComponent<TaloManager>();
            tm.settings = ScriptableObject.CreateInstance<TaloSettings>();
            tm.settings.autoConnectSocket = false;

            Talo.CurrentAlias = new PlayerAlias() {
                player = new Player() {
                    id = "uuid"
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            RequestMock.Offline = false;
        }

        [UnityTest]
        public IEnumerator DeleteSave_InOnlineMode_RemovesFromArrayOfSaves()
        {
            var api = new SavesAPI();
            Talo._saves = api;

            api._allSaves.Add(new GameSave() { id = 1, name = "Save 1" });
            api._allSaves.Add(new GameSave() { id = 2, name = "Save 2" });
            api.WriteOfflineSavesContent(new OfflineSavesContent(api._allSaves.ToArray()));

            var eventMock = new ChosenEventMock();
            api.OnSaveChosen += eventMock.Invoke;

            RequestMock.ReplyOnce(new Uri(api.GetUri() + "/1"), "DELETE");
            _ = api.DeleteSave(1);

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("Save 2", api.All[0].name);

            Assert.AreEqual(1, api.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Save 2", api.GetOfflineSavesContent().saves[0].name);

            Assert.AreEqual(null, eventMock.chosenSave);
            api.OnSaveChosen -= eventMock.Invoke;

            yield return null;
        }

        [UnityTest]
        public IEnumerator DeleteSave_InOfflineMode_RemovesFromArrayOfSaves()
        {
            RequestMock.Offline = true;

            var api = new SavesAPI();
            Talo._saves = api;

            api._allSaves.Add(new GameSave() { id = -1, name = "Save 1" });
            api._allSaves.Add(new GameSave() { id = -2, name = "Save 2" });
            api.WriteOfflineSavesContent(new OfflineSavesContent(api._allSaves.ToArray()));

            var eventMock = new ChosenEventMock();
            api.OnSaveChosen += eventMock.Invoke;

            _ = api.DeleteSave(-2);

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("Save 1", api.All[0].name);

            Assert.AreEqual(1, api.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Save 1", api.GetOfflineSavesContent().saves[0].name);

            Assert.AreEqual(null, eventMock.chosenSave);
            api.OnSaveChosen -= eventMock.Invoke;

            yield return null;
        }
    }
}
