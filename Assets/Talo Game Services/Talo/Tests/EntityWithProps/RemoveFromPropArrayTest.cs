using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace TaloGameServices.Test {
    internal class RemoveFromPropArrayTest
    {
        [UnityTest]
        public IEnumerator RemoveFromPropArray_RemovesMatchingValue()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red")),
                    new Prop(("colours[]", "blue")),
                    new Prop(("colours[]", "green"))
                }
            };

            player.RemoveFromPropArray("colours", "blue", false);

            var result = player.GetPropArray("colours").ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsFalse(result.Contains("blue"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveFromPropArray_WhenLastItemRemoved_ArrayIsEmptyAndSentinelNullExistsInProps()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red"))
                }
            };

            player.RemoveFromPropArray("colours", "red", false);

            Assert.AreEqual(0, player.GetPropArray("colours").Count);
            // ensure the sentinel null value is in there
            Assert.IsTrue(player.props.Any((prop) => prop.key == "colours[]" && prop.value == null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveFromPropArray_WhenArrayIsAlreadyDeleted_RemainsDeleted()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", "red"))
                }
            };

            player.RemoveFromPropArray("colours", "red", false);
            try                                                                                                                                                                         
            {                                                                                                                                                                           
                player.RemoveFromPropArray("colours", "red", false);                                                                                                                           
                Assert.Fail("Expected exception was not thrown");                                                                                                                       
            }                                                                                                                                                                           
            catch (Exception ex)                                                                                                                                                        
            {                                                                                                                                                                           
                Assert.AreEqual(ex.Message, "Value red does not exist in prop array colours");                                                                                 
            }   

            Assert.AreEqual(0, player.GetPropArray("colours").Count);
            Assert.IsTrue(player.props.Any((prop) => prop.key == "colours[]" && prop.value == null)); 

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveFromPropArray_WhenValueDoesNotExist_ThrowsAnError()
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
                player.RemoveFromPropArray("colours", "green", false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Value green does not exist in prop array colours");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemoveFromPropArray_WhenArrayIsAlreadyDeleted_ThrowsAnError()
        {
            var player = new Player
            {
                props = new[] {
                    new Prop(("colours[]", null))
                }
            };

            try
            {
                player.RemoveFromPropArray("colours", "red", false);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Value red does not exist in prop array colours");
            }

            yield return null;
        }
    }
}
