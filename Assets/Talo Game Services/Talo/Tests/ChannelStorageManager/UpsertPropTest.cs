using System.Threading.Tasks;
using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class UpsertPropTest
    {
        private static ChannelStorageProp MakeProp(string key, string value)
        {
            return new ChannelStorageProp { key = key, value = value };
        }

        [Test]
        public async Task UpsertProp_InsertsNewProp()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));

            var result = await manager.GetProp(1, "color");
            Assert.AreEqual("red", result.value);
        }

        [Test]
        public async Task UpsertProp_ReplacesExistingProp()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.UpsertProp(1, MakeProp("color", "blue"));

            var result = await manager.GetProp(1, "color");
            Assert.AreEqual("blue", result.value);
        }

        [Test]
        public async Task UpsertProp_LeavesOtherPropsIntact()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.UpsertProp(1, MakeProp("size", "large"));
            manager.UpsertProp(1, MakeProp("color", "blue"));

            var size = await manager.GetProp(1, "size");
            Assert.AreEqual("large", size.value);
        }

        [Test]
        public async Task UpsertProp_IsIsolatedPerChannel()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.UpsertProp(2, MakeProp("color", "blue"));

            var ch1 = await manager.GetProp(1, "color");
            var ch2 = await manager.GetProp(2, "color");
            Assert.AreEqual("red", ch1.value);
            Assert.AreEqual("blue", ch2.value);
        }

        [Test]
        public void UpsertProp_WithExpand_ExplodesJsonArrayIntoEntries()
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
        public void UpsertProp_WithExpand_PreservesMetadata()
        {
            var manager = new ChannelStorageManager();
            var alias = new PlayerAlias { id = 42 };
            var prop = new ChannelStorageProp
            {
                key = "items[]",
                value = "[\"sword\",\"shield\"]",
                createdBy = alias,
                lastUpdatedBy = alias,
                createdAt = "2024-01-01",
                updatedAt = "2024-01-02"
            };
            manager.UpsertProp(1, prop, expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(42, cached[0].createdBy.id);
            Assert.AreEqual("2024-01-02", cached[1].updatedAt);
        }

        [Test]
        public void UpsertProp_WithExpand_ReplacesExistingArrayEntries()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[\"sword\",\"shield\"]"), expand: true);
            manager.UpsertProp(1, MakeProp("items[]", "[\"bow\"]"), expand: true);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(1, cached.Length);
            Assert.AreEqual("bow", cached[0].value);
        }

        [Test]
        public async Task UpsertProp_WithExpand_EmptyJsonArray_FallsBackToSingleEntry()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[]"), expand: true);

            var result = await manager.GetProp(1, "items[]");
            Assert.AreEqual("[]", result.value);
        }

        [Test]
        public async Task UpsertProp_WithExpand_NonArrayKey_NotExpanded()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "[\"red\",\"blue\"]"), expand: true);

            var result = await manager.GetProp(1, "color");
            Assert.AreEqual("[\"red\",\"blue\"]", result.value);
        }
    }
}
