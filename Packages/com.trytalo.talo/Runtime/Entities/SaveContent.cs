using System.Collections.Generic;
using System.Linq;

namespace TaloGameServices
{
    [System.Serializable]
    public struct SavedObjectData
    {
        public string key;
        public string value;
        public string type;
    }

    [System.Serializable]
    public class SavedObject
    {
        public string id;
        public string name;
        public SavedObjectData[] data;

        public SavedObject(LoadableData loadableData)
        {
            id = loadableData.id;
            name = loadableData.name;

            if (loadableData.loadable != null)
            {
                loadableData.loadable.SavedFields.Clear();
                loadableData.loadable.RegisterFields();

                data = loadableData.loadable.SavedFields.Select((field) => new SavedObjectData()
                {
                    key = field.Key,
                    value = field.Value.ToString(),
                    type = field.Value.GetType().ToString()
                }).ToArray();
            }
            else
            {
                data = new SavedObjectData[]
                {
                    new SavedObjectData()
                    {
                        key = "meta.destroyed",
                        value = true.ToString(),
                        type = typeof(bool).ToString()
                    }
                };
            }
        }
    }

    [System.Serializable]
    public class SaveContent
    {
        public SavedObject[] objects;

        public SaveContent(List<LoadableData> loadables)
        {
            objects = loadables
                .Select((loadable) => new SavedObject(loadable))
                .ToArray();
        }
    }
}
