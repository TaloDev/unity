using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;

namespace TaloGameServices.Test {
    internal class GetPropTest
    {
        [UnityTest]
        public IEnumerator GetProp_WithALiveConfigThatHasValues_ReturnsCorrectValue()
        {
            var config = new LiveConfig(new[] { new Prop(("gameName", "Crawle")), new Prop(("halloweenEventEnabled", "True")) });

            Assert.AreEqual("Crawle", config.GetProp("gameName", "No name"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetProp_WithAnEmptyList_ReturnsFallback()
        {
            var config = new LiveConfig(Array.Empty<Prop>());

            Assert.AreEqual("No name", config.GetProp("gameName", "No name"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetProp_WithNoMatchingValuesInList_ReturnsFallback()
        {
            var config = new LiveConfig(new[] { new Prop(("gameName", "Crawle")), new Prop(("halloweenEventEnabled", "True")) });

            Assert.AreEqual("1.0", config.GetProp("latestGameVersion", "1.0"));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetProp_WhenConvertingTypeToBoolean_ReturnsCorrectValue()
        {
            var config = new LiveConfig(new[] { new Prop(("halloweenEventEnabled", "True")) });

            Assert.AreEqual(true, config.GetProp<bool>("halloweenEventEnabled", false));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetProp_WhenConvertingTypeToNumber_ReturnsCorrectValue()
        {
            var config = new LiveConfig(new[] { new Prop(("maxLevel", "60")) });

            Assert.AreEqual(60, config.GetProp<int>("maxLevel", 0));

            yield return null;
        }
    }
}
