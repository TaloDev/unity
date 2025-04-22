using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;

namespace TaloGameServices.Test
{
    internal class ChosenEventMock
    {
        public GameSave chosenSave;

        public void Invoke(GameSave chosenSave)
        {
            this.chosenSave = chosenSave;
        }
    }

    internal class CreateSaveTest
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
        public IEnumerator CreateSave_InOnlineMode_AddsToArrayOfSaves()
        {
            var api = new SavesAPI();
            Talo._saves = api;

            api._allSaves.Add(new GameSave() { name = "Existing Online Save" });
            api.WriteOfflineSavesContent(new OfflineSavesContent(api._allSaves.ToArray()));

            var eventMock = new ChosenEventMock();
            api.OnSaveChosen += eventMock.Invoke;

            RequestMock.ReplyOnce(api.GetUri(), "POST", JsonUtility.ToJson(new SavesPostResponse
            {
                save = new GameSave { id = 1, name = "New Online Save", content = "", updatedAt = "2022-10-30T21:23:30.977Z" }
            }));
            _ = api.CreateSave("New Online Save");

            Assert.AreEqual(2, api.All.Length);
            Assert.AreEqual("Existing Online Save", api.All[0].name);
            Assert.AreEqual("New Online Save", api.All[1].name);

            Assert.AreEqual(2, api.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Existing Online Save", api.GetOfflineSavesContent().saves[0].name);
            Assert.AreEqual("New Online Save", api.GetOfflineSavesContent().saves[1].name);

            Assert.AreEqual(1, api.Current.id);
            Assert.Null(eventMock.chosenSave); // should not invoke the OnSaveChosen event
            api.OnSaveChosen -= eventMock.Invoke;

            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateSave_InOfflineMode_AddsToArrayOfSaves()
        {
            RequestMock.Offline = true;

            var api = new SavesAPI();
            Talo._saves = api;

            api._allSaves.Add(new GameSave() { id = -1, name = "Existing Offline Save" });
            api.WriteOfflineSavesContent(new OfflineSavesContent(api._allSaves.ToArray()));

            var eventMock = new ChosenEventMock();
            api.OnSaveChosen += eventMock.Invoke;

            _ = api.CreateSave("New Offline Save");

            Assert.AreEqual(2, api.All.Length);
            Assert.AreEqual("Existing Offline Save", api.All[0].name);
            Assert.AreEqual("New Offline Save", api.All[1].name);

            Assert.AreEqual(2, api.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Existing Offline Save", api.GetOfflineSavesContent().saves[0].name);
            Assert.AreEqual("New Offline Save", api.GetOfflineSavesContent().saves[1].name);

            Assert.AreEqual(-2, api.Current.id);
            Assert.Null(eventMock.chosenSave); // should not invoke the OnSaveChosen event
            api.OnSaveChosen -= eventMock.Invoke;

            yield return null;
        }
    }
}
