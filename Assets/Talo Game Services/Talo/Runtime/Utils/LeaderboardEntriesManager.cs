using System;
using System.Collections.Generic;

namespace TaloGameServices
{
    public class LeaderboardEntriesManager
    {
        private Dictionary<string, List<LeaderboardEntry>> _currentEntries = new Dictionary<string, List<LeaderboardEntry>>();

        public List<LeaderboardEntry> GetEntries(string internalName)
        {
         if (!_currentEntries.ContainsKey(internalName))
            {
                _currentEntries[internalName] = new List<LeaderboardEntry>();
            }
            return _currentEntries[internalName];
        }

        public void UpsertEntry(string internalName, LeaderboardEntry entry)
        {
            if (!_currentEntries.ContainsKey(internalName))
            {
                _currentEntries[internalName] = new List<LeaderboardEntry>();
            }

            var entries = _currentEntries[internalName];
            
            // ensure there isn't an existing entry
            entries.RemoveAll((e) => e.id == entry.id);
            
            int insertPosition = FindInsertPosition(entries, entry);
            entries.Insert(insertPosition, entry);

            for (int idx = 0; idx < entries.Count; idx++)
            {
                entries[idx].position = idx;
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
