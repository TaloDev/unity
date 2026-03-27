using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TaloGameServices.Test {
    internal class SetPropArrayTest
    {
        [UnityTest]
        public IEnumerator SetPropArray_WithNewKey_AddsAllValues()
        {
            var player = new Player
            {
                props = new Prop[] { }
            };

            player.SetPropArray("colours", new [] { "red", "blue", "green" }, false);

            var result = player.GetPropArray("colours").ToList();
            Assert.AreEqual(3, result.Count);
            Assert.Contains("red", result);
            Assert.Contains("blue", result);
            Assert.Contains("green", result);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetPropArray_DeduplicatesValues()
        {
            var player = new Player
            {
                props = new Prop[] { }
            };

            player.SetPropArray("colours", new [] { "red", "red", "blue" }, false);

            var result = player.GetPropArray("colours").ToList();
            Assert.AreEqual(2, result.Count);
            Assert.Contains("red", result);
            Assert.Contains("blue", result);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetPropArray_WithAllNullOrEmptyValues_ThrowsAnError()
        {
            var player = new Player
            {
                props = new Prop[] { }
            };

            try
            {
                player.SetPropArray("colours", new string[] { null, "" }, false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Values for prop array colours must contain at least one non-empty value");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetPropArray_OnExistingPopulatedArray_ReplacesOldValues()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue"))
                }
            };

            player.SetPropArray("colours", new [] { "green", "yellow" }, false);

            var result = player.GetPropArray("colours").ToList();
            Assert.AreEqual(2, result.Count);
            Assert.Contains("green", result);
            Assert.Contains("yellow", result);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetPropArray_WithEmptyCollection_ThrowsAnError()
        {
            var player = new Player
            {
                props = new Prop[] { }
            };

            try
            {
                player.SetPropArray("colours", new string[] {}, false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Values for prop array colours must contain at least one non-empty value");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetPropArray_WhenArrayWasPreviouslyDeleted_ClearsNullEntryAndSetsNewValues()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", null))
                }
            };

            player.SetPropArray("colours", new [] { "red", "blue" }, false);

            var result = player.GetPropArray("colours").ToList();
            Assert.AreEqual(2, result.Count);
            Assert.Contains("red", result);
            Assert.Contains("blue", result);

            yield return null;
        }
    }
}
