using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TaloGameServices.Test {
    internal class DeletePropArrayTest
    {
        [UnityTest]
        public IEnumerator DeletePropArray_ArrayIsEmptyAndSentinelNullExistsInProps()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue"))
                }
            };

            player.DeletePropArray("colours", false);

            Assert.AreEqual(0, player.GetPropArray("colours").Count);
            // ensure the sentinel null value is in there
            Assert.IsTrue(player.props.Any((prop) => prop.key == "colours[]" && prop.value == null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator DeletePropArray_WhenTheArrayDoesNotExist_ThrowsAnError()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue"))
                }
            };

            try
            {
                player.DeletePropArray("sizes", false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, $"Prop array with key sizes does not exist");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator DeletePropArray_AcceptsKeyWithOrWithoutBrackets()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue"))
                }
            };

            player.DeletePropArray("colours[]", false);

            Assert.AreEqual(0, player.GetPropArray("colours").Count);
            // ensure the sentinel null value is in there
            Assert.IsTrue(player.props.Any((prop) => prop.key == "colours[]" && prop.value == null));

            yield return null;
        }
    }
}
