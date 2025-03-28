using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

namespace TaloGameServices.Test
{
    internal class PositionedLoadable : Loadable
    {
        public override void RegisterFields()
        {
            RegisterField("pos.x", transform.position.x);
            RegisterField("pos.y", transform.position.y);
            RegisterField("pos.z", transform.position.z);
        }

        public override void OnLoaded(Dictionary<string, object> data)
        {
            if (HandleDestroyed(data)) return;

            gameObject.transform.position = new Vector3(
                (float)data["pos.x"],
                (float)data["pos.y"],
                (float)data["pos.z"]
            );
        }
    }

    public class SaveContentTest
    {
        [OneTimeSetUp]
        public void Setup()
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
        public IEnumerator SaveContent_ActiveObjects_AreSavedCorrectly()
        {
            var api = new SavesAPI();
            Talo._saves = api;

            var loadable = new GameObject("First Loadable").AddComponent<PositionedLoadable>();
            loadable.transform.position = new Vector3(88, -20, 6);

            var content = new SaveContent(api._registeredLoadables);
            Assert.AreEqual(1, content.objects.Length);

            Assert.AreEqual(loadable.Id, content.objects[0].id);
            Assert.AreEqual("First Loadable", content.objects[0].name);

            Assert.AreEqual("pos.x", content.objects[0].data[0].key);
            Assert.AreEqual("88", content.objects[0].data[0].value);
            Assert.AreEqual("System.Single", content.objects[0].data[0].type);

            Assert.AreEqual("pos.y", content.objects[0].data[1].key);
            Assert.AreEqual("-20", content.objects[0].data[1].value);
            Assert.AreEqual("System.Single", content.objects[0].data[1].type);

            Assert.AreEqual("pos.z", content.objects[0].data[2].key);
            Assert.AreEqual("6", content.objects[0].data[2].value);
            Assert.AreEqual("System.Single", content.objects[0].data[2].type);

            Object.DestroyImmediate(loadable.gameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SaveContent_DestroyedObjects_AreSavedCorrectly()
        {
            var api = new SavesAPI();
            Talo._saves = api;

            var activeLoadable = new GameObject("Active Loadable").AddComponent<PositionedLoadable>();
            var destroyedLoadable = new GameObject("Destroyed Loadable").AddComponent<PositionedLoadable>();

            Object.DestroyImmediate(destroyedLoadable.gameObject);

            var content = new SaveContent(api._registeredLoadables);
            Assert.AreEqual(2, content.objects.Length);

            Assert.AreEqual(activeLoadable.Id, content.objects[0].id);
            Assert.AreEqual("Active Loadable", content.objects[0].name);
            Assert.AreEqual("pos.x", content.objects[0].data[0].key);
            Assert.AreEqual("pos.y", content.objects[0].data[1].key);
            Assert.AreEqual("pos.z", content.objects[0].data[2].key);

            Assert.AreEqual(destroyedLoadable.Id, content.objects[1].id);
            Assert.AreEqual("Destroyed Loadable", content.objects[1].name);
            Assert.AreEqual("meta.destroyed", content.objects[1].data[0].key);
            Assert.AreEqual("True", content.objects[1].data[0].value);
            Assert.AreEqual("System.Boolean", content.objects[1].data[0].type);

            Object.DestroyImmediate(activeLoadable.gameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SaveContent_NestedObjects_AreNamedCorrectly()
        {
            var api = new SavesAPI();
            Talo._saves = api;

            var grandParent = new GameObject("Grandparent");
            var parent = new GameObject("Parent");
            parent.transform.parent = grandParent.transform;

            var loadable = new GameObject("First Loadable");
            loadable.transform.parent = parent.transform;
            var id = loadable.AddComponent<PositionedLoadable>().Id;

            var content = new SaveContent(api._registeredLoadables);
            Assert.AreEqual(1, content.objects.Length);

            Assert.AreEqual(id, content.objects[0].id);
            Assert.AreEqual("Grandparent.Parent.First Loadable", content.objects[0].name);

            Object.DestroyImmediate(loadable.gameObject);

            yield return null;
        }
    }
}
