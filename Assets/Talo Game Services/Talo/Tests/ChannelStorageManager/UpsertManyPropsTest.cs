using System.Threading.Tasks;
using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class UpsertManyPropsTest
    {
        private static ChannelStorageProp MakeProp(string key, string value)
        {
            return new ChannelStorageProp { key = key, value = value };
        }

        [Test]
        public async Task UpsertManyProps_InsertsAllProps()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertManyProps(1, new[]
            {
                MakeProp("color", "red"),
                MakeProp("size", "large")
            });

            var color = await manager.GetProp(1, "color");
            var size = await manager.GetProp(1, "size");
            Assert.AreEqual("red", color.value);
            Assert.AreEqual("large", size.value);
        }

        [Test]
        public async Task UpsertManyProps_ReplacesExistingScalarProp()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.UpsertManyProps(1, new[] { MakeProp("color", "blue") });

            var result = await manager.GetProp(1, "color");
            Assert.AreEqual("blue", result.value);
        }

        [Test]
        public void UpsertManyProps_SupportsMultipleEntriesPerArrayKey()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertManyProps(1, new[]
            {
                MakeProp("items[]", "sword"),
                MakeProp("items[]", "shield"),
                MakeProp("items[]", "potion")
            });

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(3, cached.Length);
        }

        [Test]
        public void UpsertManyProps_ReplacesExistingArrayEntries()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertManyProps(1, new[]
            {
                MakeProp("items[]", "sword"),
                MakeProp("items[]", "shield")
            });
            manager.UpsertManyProps(1, new[] { MakeProp("items[]", "bow") });

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(1, cached.Length);
            Assert.AreEqual("bow", cached[0].value);
        }

        [Test]
        public async Task UpsertManyProps_LeavesOtherKeysIntact()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("size", "large"));
            manager.UpsertManyProps(1, new[]
            {
                MakeProp("items[]", "sword"),
                MakeProp("items[]", "shield")
            });

            var size = await manager.GetProp(1, "size");
            var items = manager.GetCachedProps(1);
            Assert.AreEqual("large", size.value);
            Assert.AreEqual(3, items.Length); // size + 2 items
        }
    }
}
