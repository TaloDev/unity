using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class ParseJsonStringArrayTest
    {
        private static ChannelStorageProp MakeProp(string key, string value)
        {
            return new ChannelStorageProp { key = key, value = value };
        }

        [Test]
        public void ParseJsonStringArray_NullValue_FallsBackToSingleEntry()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", null), expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(1, cached.Length);
            Assert.IsNull(cached[0].value);
        }

        [Test]
        public void ParseJsonStringArray_EmptyJsonArray_FallsBackToSingleEntry()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[]"), expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(1, cached.Length);
            Assert.AreEqual("[]", cached[0].value);
        }

        [Test]
        public void ParseJsonStringArray_SingleElement_ProducesOneEntry()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[\"sword\"]"), expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(1, cached.Length);
            Assert.AreEqual("sword", cached[0].value);
        }

        [Test]
        public void ParseJsonStringArray_MultipleElements_ProducesOneEntryPerElement()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[\"sword\",\"shield\",\"potion\"]"), expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(3, cached.Length);
            Assert.AreEqual("sword", cached[0].value);
            Assert.AreEqual("shield", cached[1].value);
            Assert.AreEqual("potion", cached[2].value);
        }

        [Test]
        public void ParseJsonStringArray_ElementsWithSpaces_TrimsWhitespace()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[\"sword\", \"shield\", \"potion\"]"), expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(3, cached.Length);
            Assert.AreEqual("sword", cached[0].value);
            Assert.AreEqual("shield", cached[1].value);
            Assert.AreEqual("potion", cached[2].value);
        }

        [Test]
        public void ParseJsonStringArray_ValuesContainingCommas_ParsesCorrectly()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[\"100,000 gold\",\"sword\"]"), expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(2, cached.Length);
            Assert.AreEqual("100,000 gold", cached[0].value);
            Assert.AreEqual("sword", cached[1].value);
        }

        [Test]
        public void ParseJsonStringArray_NumericValues_ReturnsStringRepresentations()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("scores[]", "[\"1\",\"2\",\"3\"]"), expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(3, cached.Length);
            Assert.AreEqual("1", cached[0].value);
            Assert.AreEqual("2", cached[1].value);
            Assert.AreEqual("3", cached[2].value);
        }
    }
}
