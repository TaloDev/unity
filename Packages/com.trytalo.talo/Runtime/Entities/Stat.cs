using System;

namespace TaloGameServices
{
    [Serializable]
    public class Stat
    {
        public int id;
        public string internalName;
        public string name;
        public bool global;
        public float globalValue;
        public float defaultValue;
        public float maxChange;
        public float? minValue;
        public float? maxValue;
        public int minTimeBetweenUpdates;
        public string createdAt;
        public string updatedAt;
    }
}
