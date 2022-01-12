using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public SavedObject(Loadable loadable)
        {
            id = loadable.id;

            name = GetFullName(loadable.gameObject);

            data = loadable.savedFields.Select((field) => new SavedObjectData()
            {
                key = field.Key,
                value = field.Value.ToString(),
                type = field.Value.GetType().ToString()
            }).ToArray();
        }

        private string GetFullName(GameObject go)
        {
            var name = go.name;
            while (go.transform.parent != null)
            {
                go = go.transform.parent.gameObject;
                name = $"{go.name}.{name}";
            }
            return name;
        }

    }

    [System.Serializable]
    public class SaveContent
    {
        public SavedObject[] objects;

        public SaveContent(List<Loadable> loadables)
        {
            objects = loadables
                .Select((loadable) =>
                {
                    loadable.RegisterFields();
                    return new SavedObject(loadable);
                })
                .ToArray();
        }
    }
}
