using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TaloGameServices.Test {
    internal class GetPropArrayTest
    {
        [UnityTest]
        public IEnumerator GetPropArray_WithExistingItems_ReturnsAllValues()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue")),
                    new Prop(("colours[]", "green"))
                }
            };

            var result = player.GetPropArray("colours").ToList();

            Assert.AreEqual(3, result.Count);
            Assert.Contains("red", result);
            Assert.Contains("blue", result);
            Assert.Contains("green", result);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetPropArray_WithNoMatchingKey_ReturnsEmptyList()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red"))
                }
            };

            var result = player.GetPropArray("sizes").ToList();

            Assert.AreEqual(0, result.Count);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetPropArray_AcceptsKeyWithOrWithoutBrackets()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue"))
                }
            };

            var withBrackets = player.GetPropArray("colours[]").ToList();
            var withoutBrackets = player.GetPropArray("colours").ToList();

            Assert.AreEqual(withBrackets, withoutBrackets);

            yield return null;
        }
    }
}
