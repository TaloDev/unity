using System.Threading.Tasks;
using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class OnPropsUpdatedTest
    {
        private static Channel MakeChannel(int id = 1)
        {
            return new Channel { id = id };
        }

        private static ChannelStorageProp MakeProp(string key, string value)
        {
            return new ChannelStorageProp { key = key, value = value };
        }

        [Test]
        public async Task OnPropsUpdated_UpsertsNewProps()
        {
            var manager = new ChannelStorageManager();
            manager.OnPropsUpdated(MakeChannel(), new[] { MakeProp("color", "red") }, new ChannelStorageProp[0]);

            var result = await manager.GetProp(1, "color");
            Assert.AreEqual("red", result.value);
        }

        [Test]
        public void OnPropsUpdated_DeletesProps()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.OnPropsUpdated(MakeChannel(), new ChannelStorageProp[0], new[] { MakeProp("color", "red") });

            Assert.AreEqual(0, manager.GetCachedProps(1).Length);
        }

        [Test]
        public async Task OnPropsUpdated_HandlesUpsertsAndDeletesTogether()
        {
            var manager = new ChannelStorageManager();
            manager.UpsertProp(1, MakeProp("color", "red"));
            manager.OnPropsUpdated(
                MakeChannel(),
                new[] { MakeProp("size", "large") },
                new[] { MakeProp("color", "red") }
            );

            var size = await manager.GetProp(1, "size");
            Assert.AreEqual("large", size.value);
            Assert.AreEqual(1, manager.GetCachedProps(1).Length);
        }

        [Test]
        public async Task OnPropsUpdated_UpdatesExistingPropWithoutDuplicates()
        {
            var manager = new ChannelStorageManager();
            manager.OnPropsUpdated(MakeChannel(), new[] { MakeProp("color", "red") }, new ChannelStorageProp[0]);
            manager.OnPropsUpdated(MakeChannel(), new[] { MakeProp("color", "blue") }, new ChannelStorageProp[0]);

            var result = await manager.GetProp(1, "color");
            Assert.AreEqual("blue", result.value);
            Assert.AreEqual(1, manager.GetCachedProps(1).Length);
        }

        [Test]
        public void OnPropsUpdated_UpdatesExistingArrayPropWithoutDuplicates()
        {
            var manager = new ChannelStorageManager();
            manager.OnPropsUpdated(MakeChannel(), new[] { MakeProp("items[]", "sword"), MakeProp("items[]", "shield") }, new ChannelStorageProp[0]);
            manager.OnPropsUpdated(MakeChannel(), new[] { MakeProp("items[]", "sword"), MakeProp("items[]", "bow") }, new ChannelStorageProp[0]);

            var cached = manager.GetCachedProps(1);
            Assert.AreEqual(2, cached.Length);
            Assert.AreEqual("sword", cached[0].value);
            Assert.AreEqual("bow", cached[1].value);
        }

        [Test]
        public async Task OnPropsUpdated_UsesChannelIdForIsolation()
        {
            var manager = new ChannelStorageManager();
            manager.OnPropsUpdated(MakeChannel(1), new[] { MakeProp("color", "red") }, new ChannelStorageProp[0]);
            manager.OnPropsUpdated(MakeChannel(2), new[] { MakeProp("color", "blue") }, new ChannelStorageProp[0]);

            var ch1 = await manager.GetProp(1, "color");
            var ch2 = await manager.GetProp(2, "color");
            Assert.AreEqual("red", ch1.value);
            Assert.AreEqual("blue", ch2.value);
        }
    }
}
