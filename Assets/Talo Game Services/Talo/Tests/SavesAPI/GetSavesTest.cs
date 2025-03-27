using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System;

namespace TaloGameServices.Test
{
    internal class LoadedEventMock
    {
        public bool wasInvoked;

        public void Invoke()
        {
            wasInvoked = true;
        }
    }

    internal class GetSavesTest
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
        public IEnumerator GetSaves_InOnlineMode_ReturnsAnArrayOfSaves()
        {
            var api = new SavesAPI();

            var eventMock = new LoadedEventMock();
            api.OnSavesLoaded += eventMock.Invoke; 

            RequestMock.ReplyOnce(api.GetUri(), "GET", JsonUtility.ToJson(new SavesIndexResponse
            {
                saves = new GameSave[] {
                    new GameSave {
                        id = 1,
                        name = "Online Save",
                        content = "",
                        updatedAt = "2022-10-30T21:23:30.977Z"
                    }
                }
            }));
            _ = api.GetSaves();

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("Online Save", api.All[0].name);

            Assert.AreEqual(1, api.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Online Save", api.GetOfflineSavesContent().saves[0].name);

            Assert.IsTrue(eventMock.wasInvoked);
            api.OnSavesLoaded -= eventMock.Invoke;

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetSaves_InOfflineModeWithNoSaves_ReturnsAnEmptyArray()
        {
            RequestMock.Offline = true;

            var api = new SavesAPI();

            var eventMock = new LoadedEventMock();
            api.OnSavesLoaded += eventMock.Invoke;

            _ = api.GetSaves();

            Assert.AreEqual(0, api.All.Length);
            Assert.IsNull(api.GetOfflineSavesContent());

            Assert.IsTrue(eventMock.wasInvoked);
            api.OnSavesLoaded -= eventMock.Invoke;

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetSaves_InOfflineModeWithSaves_ReturnsAnArrayOfSaves()
        {
            RequestMock.Offline = true;

            var api = new SavesAPI();
            _ = api.CreateSave("Offline Save");

            var eventMock = new LoadedEventMock();
            api.OnSavesLoaded += eventMock.Invoke;

            _ = api.GetSaves();

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("Offline Save", api.All[0].name);

            Assert.AreEqual(1, api.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Offline Save", api.GetOfflineSavesContent().saves[0].name);

            Assert.IsTrue(eventMock.wasInvoked);
            api.OnSavesLoaded -= eventMock.Invoke;

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetSaves_InOnlineMode_PrefersTheLastUpdatedSaveIfTwoWithTheSameIDExist()
        {
            var api = new SavesAPI();

            var eventMock = new LoadedEventMock();
            api.OnSavesLoaded += eventMock.Invoke;

            api.WriteOfflineSavesContent(new OfflineSavesContent(
                new GameSave[] {
                    new GameSave
                    {
                        id = 2,
                        name = "Both Save (old)",
                        content = "",
                        updatedAt = "2022-10-29T21:23:30.977Z"
                    }
                }
            ));

            RequestMock.ReplyOnce(api.GetUri(), "GET", JsonUtility.ToJson(new SavesIndexResponse
            {
                saves = new GameSave[] {
                    new GameSave {
                        id = 2,
                        name = "Both Save (new)",
                        content = "",
                        updatedAt = "2022-10-30T21:23:30.977Z"
                    }
                }
            }));
            _ = api.GetSaves();

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("Both Save (new)", api.All[0].name);

            Assert.AreEqual(1, api.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Both Save (new)", api.GetOfflineSavesContent().saves[0].name);
            Assert.AreEqual("2022-10-30T21:23:30.977Z", api.GetOfflineSavesContent().saves[0].updatedAt);

            Assert.IsTrue(eventMock.wasInvoked);
            api.OnSavesLoaded -= eventMock.Invoke;

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetSaves_InOnlineMode_ReplacesTheOnlineSaveIfTheOfflineVersionIsNewer()
        {
            var api = new SavesAPI();

            var eventMock = new LoadedEventMock();
            api.OnSavesLoaded += eventMock.Invoke;

            api.WriteOfflineSavesContent(new OfflineSavesContent(
                new GameSave[] {
                    new GameSave
                    {
                        id = 3,
                        name = "Both Save (offline)",
                        content = "",
                        updatedAt = "2022-10-30T21:23:30.977Z"
                    }
                }
            ));

            RequestMock.ReplyOnce(api.GetUri(), "GET", JsonUtility.ToJson(new SavesIndexResponse
            {
                saves = new GameSave[] {
                    new GameSave {
                        id = 3,
                        name = "Both Save (online)",
                        content = "",
                        updatedAt = "2022-10-29T21:23:30.977Z"
                    }
                }
            }));

            RequestMock.ReplyOnce(new Uri(api.GetUri() + "/3"), "PATCH", JsonUtility.ToJson(new SavesPostResponse
            {
                save = new GameSave {
                    id = 3,
                    name = "Both Save (offline)",
                    content = "",
                    updatedAt = "2022-10-30T21:30:30.977Z"
                }
            }));

            _ = api.GetSaves();

            Assert.AreEqual(1, api.All.Length);
            Assert.AreEqual("Both Save (offline)", api.All[0].name);

            Assert.AreEqual(1, api.GetOfflineSavesContent().saves.Length);
            Assert.AreEqual("Both Save (offline)", api.GetOfflineSavesContent().saves[0].name);
            Assert.AreEqual("2022-10-30T21:30:30.977Z", api.GetOfflineSavesContent().saves[0].updatedAt);

            Assert.IsTrue(eventMock.wasInvoked);
            api.OnSavesLoaded -= eventMock.Invoke;

            yield return null;
        }
    }
}
