using System;
using System.Collections.Generic;

namespace TaloGameServices
{
    public class SavesContentManager
    {
        private Dictionary<string, Loadable> loadables = new();
        internal Dictionary<string, SavedObject> savedObjects = new();

        public event Action OnSaveLoadingCompleted;

        public SaveContent Content => new(savedObjects);

        public SavesContentManager()
        {
            Talo.Saves.OnSaveChosen += MatchLoadables;
            Talo.Saves.OnSaveUnloaded += (_save) =>
            {
                loadables.Clear();
                savedObjects.Clear();
            };
        }

        internal void Register(Loadable loadable)
        {
            loadables[loadable.Id] = loadable;

            // create a new saved object in case it isn't in the save file yet
            if (savedObjects.TryGetValue(loadable.Id, out var existingSavedObject))
            {
                existingSavedObject.RegisterLoadable(loadable);
            }
            else
            {
                var savedObject = new SavedObject(loadable.Id, loadable.GetPath(), loadable.GetLatestData());
                savedObject.RegisterLoadable(loadable, false); // no need to hydrate, the data will match
                savedObjects[loadable.Id] = savedObject;
            }
        }

        private void MatchLoadables(GameSave save)
        {
            foreach (var item in save.content.objects)
            {
                var savedObject = new SavedObject(item.id, item.name, item.data);
                savedObjects[savedObject.id] = savedObject;

                var matchingLoadable = loadables.GetValueOrDefault(savedObject.id);
                if (matchingLoadable != null)
                {
                    savedObject.RegisterLoadable(matchingLoadable);
                }
            }

            OnSaveLoadingCompleted?.Invoke();
        }
    }
}
