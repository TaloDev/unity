using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System.Collections.Generic;

namespace TaloGameServices.Test
{
    internal class LoadingCompletedEvent
    {
        public bool wasInvoked;

        public void Invoke()
        {
            wasInvoked = true;
        }
    }

    internal class ChooseSaveTest
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

        [UnityTest]
        public IEnumerator ChooseSave_LoadsLoadableData()
        {
            var api = new SavesAPI();
            Talo._saves = api;
            api.Setup();

            Vector3 loadable1Pos, loadable2Pos;

            var loadable1 = new GameObject("First Loadable").AddComponent<PositionedLoadable>();
            loadable1.transform.position = loadable1Pos = new Vector3(88, -20, 6);
            var loadable2 = new GameObject("Second Loadable").AddComponent<PositionedLoadable>();
            loadable2.transform.position = loadable2Pos = new Vector3(-4, 105, 71);

            var savedObjects = new Dictionary<string, SavedObject>
            {
                { loadable1.Id, new SavedObject(loadable1.Id, loadable1.GetPath(), loadable1.GetLatestData()) },
                { loadable2.Id, new SavedObject(loadable2.Id, loadable2.GetPath(), loadable2.GetLatestData()) }
            };

            api.savesManager._allSaves.Add(new GameSave() {
                id = 1,
                name = "Save",
                content = new SaveContent(savedObjects)
            });

            loadable1.transform.position = Vector3.zero;
            loadable2.transform.position = Vector3.zero;

            var eventMock = new LoadingCompletedEvent();
            api.OnSaveLoadingCompleted += eventMock.Invoke;

            api.ChooseSave(api.All[0].id);

            Assert.AreEqual(loadable1.transform.position, loadable1Pos);
            Assert.AreEqual(loadable2.transform.position, loadable2Pos);

            api.OnSaveLoadingCompleted -= eventMock.Invoke;

            yield return null;
        }
    }
}
