using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class LeaderboardEntriesManagerTests
    {
        private LeaderboardEntriesManager manager;

        [SetUp]
        public void SetUp()
        {
            manager = new LeaderboardEntriesManager();
        }

        [Test]
        public void UpsertEntry_DescendingSort_InsertsInCorrectPosition()
        {
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, leaderboardSortMode = "desc" };
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, leaderboardSortMode = "desc" };
            var entry3 = new LeaderboardEntry { id = 3, score = 90f, leaderboardSortMode = "desc" };

            manager.UpsertEntry("test", entry1);
            manager.UpsertEntry("test", entry2);
            manager.UpsertEntry("test", entry3);

            var entries = manager.GetEntries("test");
            
            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual(100f, entries[0].score);
            Assert.AreEqual(90f, entries[1].score);
            Assert.AreEqual(80f, entries[2].score);
            
            Assert.AreEqual(0, entries[0].position);
            Assert.AreEqual(1, entries[1].position);
            Assert.AreEqual(2, entries[2].position);
        }

        [Test]
        public void UpsertEntry_AscendingSort_InsertsInCorrectPosition()
        {
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, leaderboardSortMode = "asc" };
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, leaderboardSortMode = "asc" };
            var entry3 = new LeaderboardEntry { id = 3, score = 90f, leaderboardSortMode = "asc" };

            manager.UpsertEntry("test", entry1);
            manager.UpsertEntry("test", entry2);
            manager.UpsertEntry("test", entry3);

            var entries = manager.GetEntries("test");
            
            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual(80f, entries[0].score);
            Assert.AreEqual(90f, entries[1].score);
            Assert.AreEqual(100f, entries[2].score);
            
            Assert.AreEqual(0, entries[0].position);
            Assert.AreEqual(1, entries[1].position);
            Assert.AreEqual(2, entries[2].position);
        }

        [Test]
        public void UpsertEntry_UpdateExistingEntry_MaintainsCorrectOrder()
        {
            // highest score
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", entry1);
            
            // should go after entry1
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", entry2);
            
            // update entry1 to have the lowest score - should move to end
            var updatedEntry1 = new LeaderboardEntry { id = 1, score = 70f, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", updatedEntry1);

            var entries = manager.GetEntries("test");            
            Assert.AreEqual(2, entries.Count);

            Assert.AreEqual(2, entries[0].id); // entry2 should be first
            Assert.AreEqual(80f, entries[0].score);

            Assert.AreEqual(1, entries[1].id); // updated entry1 should be second  
            Assert.AreEqual(70f, entries[1].score);
            
            Assert.AreEqual(0, entries[0].position);
            Assert.AreEqual(1, entries[1].position);
        }

        [Test]
        public void UpsertEntry_EmptyList_InsertsFirstEntry()
        {
            var entry = new LeaderboardEntry { id = 1, score = 100f, leaderboardSortMode = "desc" };

            manager.UpsertEntry("test", entry);

            var entries = manager.GetEntries("test");
            
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual(100f, entries[0].score);
            Assert.AreEqual(0, entries[0].position);
        }

        [Test]
        public void UpsertEntry_EqualScores_OrdersByCreatedAt()
        {
            var earlierEntry = new LeaderboardEntry 
            { 
                id = 1, 
                score = 100f, 
                leaderboardSortMode = "desc",
                createdAt = "2025-09-13T10:00:00Z"
            };
            var laterEntry = new LeaderboardEntry 
            { 
                id = 2, 
                score = 100f, 
                leaderboardSortMode = "desc",
                createdAt = "2025-09-13T11:00:00Z"
            };

            manager.UpsertEntry("test", laterEntry);
            manager.UpsertEntry("test", earlierEntry);

            var entries = manager.GetEntries("test");
            
            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual(1, entries[0].id); // earlier entry should be first
            Assert.AreEqual(2, entries[1].id); // later entry should be second
            Assert.AreEqual(0, entries[0].position);
            Assert.AreEqual(1, entries[1].position);
        }
    }
}