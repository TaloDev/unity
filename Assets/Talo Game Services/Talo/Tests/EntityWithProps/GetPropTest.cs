using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TaloGameServices.Test {
    internal class GetPropTest
    {
        [UnityTest]
        public IEnumerator GetProp_WithAnExistingKey_ReturnsCorrectValue()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("key1", "value1")),
                    new Prop(("key2", "value2"))
                }
            };

            Assert.AreEqual(player.GetProp("key2"), "value2");

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetProp_WithAMissingKey_ReturnsNull()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("key1", "value1")),
                    new Prop(("key2", "value2"))
                }
            };

            Assert.IsNull(player.GetProp("key3"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetProp_WithAMissingKeyAndFallback_ReturnsNull()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("key1", "value1")),
                    new Prop(("key2", "value2"))
                }
            };

            Assert.AreEqual(player.GetProp("key3", "value3"), "value3");

            yield return null;
        }
    }
}
