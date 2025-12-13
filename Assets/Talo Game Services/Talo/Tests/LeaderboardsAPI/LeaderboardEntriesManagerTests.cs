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
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, position = 0, leaderboardSortMode = "desc" };
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, position = 2, leaderboardSortMode = "desc" };
            var entry3 = new LeaderboardEntry { id = 3, score = 90f, position = 1, leaderboardSortMode = "desc" };

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
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, position = 2, leaderboardSortMode = "asc" };
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, position = 0, leaderboardSortMode = "asc" };
            var entry3 = new LeaderboardEntry { id = 3, score = 90f, position = 1, leaderboardSortMode = "asc" };

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
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, position = 0, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", entry1);

            // should go after entry1
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, position = 1, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", entry2);

            // update entry1 to have the lowest score - should move to end
            var updatedEntry1 = new LeaderboardEntry { id = 1, score = 70f, position = 1, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", updatedEntry1);

            var entries = manager.GetEntries("test");
            Assert.AreEqual(2, entries.Count);

            Assert.AreEqual(2, entries[0].id); // entry2 should be first
            Assert.AreEqual(80f, entries[0].score);

            Assert.AreEqual(1, entries[1].id); // updated entry1 should be second
            Assert.AreEqual(70f, entries[1].score);

            // Positions preserved as-is when not bumping (intentional)
            Assert.AreEqual(1, entries[0].position);
            Assert.AreEqual(1, entries[1].position);
        }

        [Test]
        public void UpsertEntry_EmptyList_InsertsFirstEntry()
        {
            var entry = new LeaderboardEntry { id = 1, score = 100f, position = 0, leaderboardSortMode = "desc" };

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
                position = 0,
                leaderboardSortMode = "desc",
                createdAt = "2025-09-13T10:00:00Z"
            };
            var laterEntry = new LeaderboardEntry
            {
                id = 2,
                score = 100f,
                position = 1,
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

        [Test]
        public void UpsertEntry_BumpPositions_BumpsExistingEntries()
        {
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, position = 0, leaderboardSortMode = "desc" };
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, position = 1, leaderboardSortMode = "desc" };

            manager.UpsertEntry("test", entry1);
            manager.UpsertEntry("test", entry2);

            // Insert a new entry with position 0 - should bump the others
            var entry3 = new LeaderboardEntry { id = 3, score = 110f, position = 0, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", entry3, bumpPositions: true);

            var entries = manager.GetEntries("test");

            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual(3, entries[0].id);
            Assert.AreEqual(0, entries[0].position); // new entry at position 0

            Assert.AreEqual(1, entries[1].id);
            Assert.AreEqual(1, entries[1].position); // bumped from 0 to 1

            Assert.AreEqual(2, entries[2].id);
            Assert.AreEqual(2, entries[2].position); // bumped from 1 to 2
        }

        [Test]
        public void UpsertEntry_BumpPositions_OnlyBumpsAffectedEntries()
        {
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, position = 0, leaderboardSortMode = "desc" };
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, position = 1, leaderboardSortMode = "desc" };
            var entry3 = new LeaderboardEntry { id = 3, score = 60f, position = 2, leaderboardSortMode = "desc" };

            manager.UpsertEntry("test", entry1);
            manager.UpsertEntry("test", entry2);
            manager.UpsertEntry("test", entry3);

            // Insert a new entry at position 1 - should only bump entries at position >= 1
            var entry4 = new LeaderboardEntry { id = 4, score = 90f, position = 1, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", entry4, bumpPositions: true);

            var entries = manager.GetEntries("test");

            Assert.AreEqual(4, entries.Count);

            Assert.AreEqual(1, entries[0].id);
            Assert.AreEqual(0, entries[0].position); // unchanged

            Assert.AreEqual(4, entries[1].id);
            Assert.AreEqual(1, entries[1].position); // new entry at position 1

            Assert.AreEqual(2, entries[2].id);
            Assert.AreEqual(2, entries[2].position); // bumped from 1 to 2

            Assert.AreEqual(3, entries[3].id);
            Assert.AreEqual(3, entries[3].position); // bumped from 2 to 3
        }

        [Test]
        public void UpsertEntry_NoBumpPositions_PreservesExistingPositions()
        {
            var entry1 = new LeaderboardEntry { id = 1, score = 100f, position = 0, leaderboardSortMode = "desc" };
            var entry2 = new LeaderboardEntry { id = 2, score = 80f, position = 1, leaderboardSortMode = "desc" };

            manager.UpsertEntry("test", entry1);
            manager.UpsertEntry("test", entry2);

            // Insert a new entry with position 0 without bumping
            var entry3 = new LeaderboardEntry { id = 3, score = 110f, position = 0, leaderboardSortMode = "desc" };
            manager.UpsertEntry("test", entry3, bumpPositions: false);

            var entries = manager.GetEntries("test");

            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual(3, entries[0].id);
            Assert.AreEqual(0, entries[0].position); // new entry at position 0

            Assert.AreEqual(1, entries[1].id);
            Assert.AreEqual(0, entries[1].position); // unchanged - still position 0

            Assert.AreEqual(2, entries[2].id);
            Assert.AreEqual(1, entries[2].position); // unchanged - still position 1
        }
    }
}