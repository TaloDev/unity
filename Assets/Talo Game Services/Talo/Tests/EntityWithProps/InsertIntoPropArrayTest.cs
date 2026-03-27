using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TaloGameServices.Test {
    internal class InsertIntoPropArrayTest
    {
        [UnityTest]
        public IEnumerator InsertIntoPropArray_AddsValueToExistingArray()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue"))
                }
            };

            player.InsertIntoPropArray("colours", "green", false);

            var result = player.GetPropArray("colours").ToList();
            Assert.AreEqual(3, result.Count);
            Assert.Contains("green", result);

            yield return null;
        }

        [UnityTest]
        public IEnumerator InsertIntoPropArray_DoesNotAddDuplicateValue()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue"))
                }
            };

            player.InsertIntoPropArray("colours", "red", false);

            var result = player.GetPropArray("colours").ToList();
            Assert.AreEqual(2, result.Count);

            yield return null;
        }

        [UnityTest]
        public IEnumerator InsertIntoPropArray_WhenArrayWasPreviouslyDeleted_ClearsNullEntryAndInsertsValue()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", null))
                }
            };

            player.InsertIntoPropArray("colours", "red", false);

            var result = player.GetPropArray("colours").ToList();
            Assert.AreEqual(1, result.Count);
            Assert.Contains("red", result);

            yield return null;
        }

        [UnityTest]
        public IEnumerator InsertIntoPropArray_WithNullValue_ThrowsAnError()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red"))
                }
            };

            try
            {
                player.InsertIntoPropArray("colours", null, false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Value for prop array colours cannot be null or empty");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator InsertIntoPropArray_WithEmptyValue_ThrowsAnError()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red"))
                }
            };

            try
            {
                player.InsertIntoPropArray("colours", "", false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Value for prop array colours cannot be null or empty");
            }

            yield return null;
        }
    }
}
