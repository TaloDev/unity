using System;
using System.Collections.Generic;
using System.Linq;

namespace TaloGameServices
{
    [Serializable]
    public struct SaveContentObject
    {
        public string id;
        public string name;
        public SavedObjectData[] data;
    }

    [Serializable]
    public class SaveContent
    {
        public SaveContentObject[] objects;

        public SaveContent(Dictionary<string, SavedObject> savedObjects)
        {
            objects = savedObjects
                .Select((item) =>
                {
                    var savedObject = item.Value;
                    return new SaveContentObject()
                    {
                        id = savedObject.id,
                        name = savedObject.name,
                        data = savedObject.SerialiseData()
                    };
                })
                .ToArray();
        }
    }
}
