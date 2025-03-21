using System;

namespace TaloGameServices
{
    [Serializable]
    public class PlayerStatSnapshot
    {
        public float change;
        public float value;
        public float globalValue;
        public string createdAt;
    }
}
