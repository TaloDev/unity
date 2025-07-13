using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System.Collections.Generic;

namespace TaloGameServices.Test
{
    internal class UnloadedEventMock
    {
        private int expectedSaveId;
        public bool wasInvoked;

        public UnloadedEventMock(int expectedSaveId)
        {
            this.expectedSaveId = expectedSaveId;
        }

        public void Invoke(GameSave save)
        {
            wasInvoked = save.id == expectedSaveId;
        }
    }

    internal class UnloadSaveTest
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
        public IEnumerator UnloadCurrentSave_InvokesOnSaveUnloaded()
        {
            var api = new SavesAPI();
            Talo._saves = api;
            api.Setup();

            var loadable1 = new GameObject("First Loadable").AddComponent<PositionedLoadable>();
            var loadable2 = new GameObject("Second Loadable").AddComponent<PositionedLoadable>();
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

            var unloadedEventMock = new UnloadedEventMock(api.All[0].id);
            api.OnSaveUnloaded += unloadedEventMock.Invoke;

            api.ChooseSave(api.All[0].id);

            var chosenEventMock = new ChosenEventMock();
            api.OnSaveChosen += chosenEventMock.Invoke;

            api.UnloadCurrentSave();
            Assert.Null(chosenEventMock.chosenSave);
            Assert.True(unloadedEventMock.wasInvoked);
            Assert.Null(api.Current);

            api.OnSaveUnloaded -= unloadedEventMock.Invoke;
            api.OnSaveChosen -= chosenEventMock.Invoke;

            yield return null;
        }
    }
}
