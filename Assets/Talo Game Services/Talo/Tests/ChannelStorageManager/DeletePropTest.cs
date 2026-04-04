using System.Threading.Tasks;
using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class DeletePropTest
    {
        private static ChannelStorageProp MakeProp(string key, string value)
        {
            return new ChannelStorageProp { key = key, value = value };
        }

        [Test]
        public void DeleteProp_RemovesProp()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.DeleteProp(1, "color");

            Assert.AreEqual(0, manager.GetCachedProps(1).Length);
        }

        [Test]
        public async Task DeleteProp_LeavesOtherPropsIntact()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.UpsertProp(1, MakeProp("size", "large"));
            manager.DeleteProp(1, "color");

            var size = await manager.GetProp(1, "size");
            Assert.AreEqual("large", size.value);
        }

        [Test]
        public void DeleteProp_RemovesAllEntriesForArrayKey()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[\"sword\",\"shield\"]"), expand: true);
            manager.DeleteProp(1, "items[]");

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(0, cached.Length);
        }

        [Test]
        public void DeleteProp_LeavesOtherArrayPropsIntact()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("items[]", "[\"sword\",\"shield\"]"), expand: true);
            manager.UpsertProp(1, MakeProp("tags[]", "[\"rpg\",\"fantasy\"]"), expand: true);
            manager.DeleteProp(1, "items[]");

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(2, cached.Length);
            Assert.AreEqual("rpg", cached[0].value);
            Assert.AreEqual("fantasy", cached[1].value);
        }

        [Test]
        public void DeleteProp_IsIsolatedPerChannel()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.UpsertProp(2, MakeProp("color", "blue"));
            manager.DeleteProp(1, "color");

            Assert.AreEqual(0, manager.GetCachedProps(1).Length);
            Assert.AreEqual("blue", manager.GetCachedProps(2)[0].value);
        }
    }
}
