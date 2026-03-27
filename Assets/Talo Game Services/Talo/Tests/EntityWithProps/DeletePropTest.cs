using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TaloGameServices.Test {
    internal class DeleteProp
    {
        [UnityTest]
        public IEnumerator DeleteProp_WhenThePropExists_SetsTheValueToNull()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("key1", "value1")),
                    new Prop(("key2", "value2"))
                }
            };

            player.DeleteProp("key2", false);

            Assert.IsNull(player.GetProp("key2"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator DeleteProp_WhenThePropDoesNotExist_ThrowsAnError()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("key1", "value1")),
                    new Prop(("key2", "value2"))
                }
            };

            try
            {
                player.DeleteProp("key3", false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, $"Prop with key key3 does not exist");
            }

            yield return null;
        }

    }
}
