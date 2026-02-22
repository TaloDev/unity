using System;
using System.Collections.Generic;

namespace TaloGameServices
{
    public class LeaderboardEntriesManager
    {
        private readonly Dictionary<string, List<LeaderboardEntry>> _currentEntries = new();

        public List<LeaderboardEntry> GetEntries(string internalName)
        {
         if (!_currentEntries.ContainsKey(internalName))
            {
                _currentEntries[internalName] = new List<LeaderboardEntry>();
            }
            return _currentEntries[internalName];
        }

        public void UpsertEntry(string internalName, LeaderboardEntry upsertEntry, bool bumpPositions = false)
        {
            if (!_currentEntries.ContainsKey(internalName))
            {
                _currentEntries[internalName] = new List<LeaderboardEntry>();
            }

            var entries = _currentEntries[internalName];
            
            // ensure there isn't an existing entry
            entries.RemoveAll((e) => e.id == upsertEntry.id);
            
            int insertPosition = FindInsertPosition(entries, upsertEntry);
            entries.Insert(insertPosition, upsertEntry);

            if (bumpPositions)
            {
                // if we find a collision, bump subsequent entries down by 1
                int collisionIndex = entries.FindIndex((e) => e.id != upsertEntry.id && e.position == upsertEntry.position);
                if (collisionIndex != -1)
                {
                    for (int i = collisionIndex; i < entries.Count; i++)
                    {
                        if (entries[i].id != upsertEntry.id)
                        {
                            entries[i].position += 1;
                        }
                    }
                }
            }
        }

        private int FindInsertPosition(List<LeaderboardEntry> entries, LeaderboardEntry newEntry)
        {
            if (entries.Count == 0)
            {
                return 0;
            }

            int left = 0;
            int right = entries.Count;

            while (left < right)
            {
                int mid = left + (right - left) / 2;
                if (CompareEntries(newEntry, entries[mid]))
                {
                    right = mid;
                }
                else
                {
                    left = mid + 1;
                }
            }

            return left;
        }

        private bool CompareEntries(LeaderboardEntry a, LeaderboardEntry b)
        {
            // first compare by score based on sort mode
            if (a.score != b.score)
            {
                if (a.LeaderboardSortMode == LeaderboardSortMode.ASC)
                {
                    return a.score < b.score;
                }
                else
                {
                    return a.score > b.score;
                }
            }

            // if scores are equal, earlier entries win
            return DateTime.Parse(a.createdAt) < DateTime.Parse(b.createdAt);
        }
    }
}
