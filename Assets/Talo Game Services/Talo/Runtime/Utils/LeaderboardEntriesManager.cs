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
            else
            {
                _currentEntries[internalName].RemoveAll(e => e.id == entry.id);
            }

            if (entry.position >= _currentEntries[internalName].Count)
            {
                _currentEntries[internalName].Add(entry);
            }
            else
            {
                _currentEntries[internalName].Insert(entry.position, entry);
            }

            for (int idx = entry.position; idx < _currentEntries[internalName].Count; idx++)
            {
                _currentEntries[internalName][idx].position = idx;
            }
        }
    }
}
