using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System;

namespace TaloGameServices.Test
{
    internal class UpdateSaveTest
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
        public IEnumerator UpdateSave_InOnlineMode_UpdatesTheSaveContent()
        {
            var api = new SavesAPI();
            Talo._saves = api;

            api.savesManager._allSaves.Add(new GameSave() { id = 1, name = "Online Save" });
            api.savesManager.WriteOfflineSavesContent(new OfflineSavesContent(api.savesManager._allSaves.ToArray()));

            RequestMock.ReplyOnce(new Uri(api.GetUri() + "/1"), "PATCH", JsonUtility.ToJson(new SavesPostResponse
            {
                save = new GameSave
                {
                    id = 1,
                    name = "Online Save",
                    content = JsonUtility.FromJson<SaveContent>("updated"),
                    updatedAt = "2022-10-30T21:23:30.977Z"
                }
            }));
            _ = api.UpdateSave(1);

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("Online Save", api.All[0].name);
            Assert.AreEqual("updated", api.All[0].content);

            Assert.AreEqual(1, api.savesManager.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Online Save", api.savesManager.GetOfflineSavesContent().saves[0].name);
            Assert.AreEqual("updated", api.savesManager.GetOfflineSavesContent().saves[0].content);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateSave_InOnlineMode_UpdatesTheSaveName()
        {
            var api = new SavesAPI();
            Talo._saves = api;

            api.savesManager._allSaves.Add(new GameSave() { id = 1, name = "Online Save" });
            api.savesManager.WriteOfflineSavesContent(new OfflineSavesContent(api.savesManager._allSaves.ToArray()));

            RequestMock.ReplyOnce(new Uri(api.GetUri() + "/1"), "PATCH", JsonUtility.ToJson(new SavesPostResponse
            {
                save = new GameSave
                {
                    id = 1,
                    name = "New Name",
                    content = JsonUtility.FromJson<SaveContent>(""),
                    updatedAt = "2022-10-30T21:23:30.977Z"
                }
            }));
            _ = api.UpdateSave(1);

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("New Name", api.All[0].name);

            Assert.AreEqual(1, api.savesManager.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("New Name", api.savesManager.GetOfflineSavesContent().saves[0].name);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateSave_InOfflineMode_UpdatesTheSaveContent()
        {
            RequestMock.Offline = true;

            var api = new SavesAPI();
            Talo._saves = api;

            api.savesManager._allSaves.Add(new GameSave() { id = -1, name = "Offline Save" });
            api.savesManager.WriteOfflineSavesContent(new OfflineSavesContent(api.savesManager._allSaves.ToArray()));

            _ = api.UpdateSave(-1);

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("Offline Save", api.All[0].name);

            Assert.AreEqual(1, api.savesManager.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Offline Save", api.savesManager.GetOfflineSavesContent().saves[0].name);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UpdateSave_InOfflineMode_UpdatesTheSaveName()
        {
            RequestMock.Offline = true;

            var api = new SavesAPI();
            Talo._saves = api;

            api.savesManager._allSaves.Add(new GameSave() { id = -1, name = "Offline Save" });
            api.savesManager.WriteOfflineSavesContent(new OfflineSavesContent(api.savesManager._allSaves.ToArray()));

            _ = api.UpdateSave(-1, "New Name");

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("New Name", api.All[0].name);

            Assert.AreEqual(1, api.savesManager.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("New Name", api.savesManager.GetOfflineSavesContent().saves[0].name);

            yield return null;
        }
    }
}
