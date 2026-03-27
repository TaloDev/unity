using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TaloGameServices.Test {
    internal class SetPropTest
    {
        [UnityTest]
        public IEnumerator SetProp_WhenPropDoesNotAlreadyExist_AppendsTheNewProp()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("key1", "value1")),
                    new Prop(("key2", "value2"))
                }
            };

            player.SetProp("key3", "value3", false);

            Assert.AreEqual(player.GetProp("key3"), "value3");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetProp_WhenPropAlreadyExists_UpdatesTheProp()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("key1", "value1")),
                    new Prop(("key2", "value2"))
                }
            };

            player.SetProp("key2", "2value2updated", false);

            Assert.AreEqual(player.GetProp("key2"), "2value2updated");

            yield return null;
        }
    }
}
