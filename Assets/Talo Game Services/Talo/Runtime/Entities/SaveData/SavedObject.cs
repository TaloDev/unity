using System;
using System.Linq;
using UnityEngine.SceneManagement;

namespace TaloGameServices
{
    [Serializable]
    public struct SavedObjectData
    {
        public string key;
        public string value;
        public string type;
    }

    public class SavedObject
    {
        internal string id;
        internal string name;
        private Loadable loadable;
        private SavedObjectData[] cachedData;
        private int sceneIndex = -1;

        public SavedObject(string id, string name, SavedObjectData[] data)
        {
            this.id = id;
            this.name = name;
            cachedData = data;
        }

        private SavedObjectData[] GetLatestData()
        {
            cachedData = loadable.GetLatestData();
            return cachedData;
        }

        private bool IsLoadableValid()
        {
            return loadable != null;
        }

        private int GetCurrentSceneIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        private bool CurrentSceneMatches()
        {
            return sceneIndex >= 0 && GetCurrentSceneIndex() == sceneIndex;
        }

        internal void RegisterLoadable(Loadable loadable, bool hydrate = true)
        {
            this.loadable = loadable;
            sceneIndex = GetCurrentSceneIndex();

            if (hydrate)
            {
                loadable.Hydrate(cachedData);
            }
        }

        internal SavedObjectData[] SerialiseData()
        {
            var valid = IsLoadableValid();

            if (!valid && !CurrentSceneMatches())
            {
                return cachedData;
            }

            if (valid)
            {
                return GetLatestData();
            }

            cachedData = cachedData.Append(new SavedObjectData()
            {
                key = "meta.destroyed",
                value = true.ToString(),
                type = typeof(bool).ToString()
            }).ToArray();

            return cachedData;
        }
    }
}
