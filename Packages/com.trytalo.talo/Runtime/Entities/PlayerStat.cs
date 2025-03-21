using System;

namespace TaloGameServices
{
    [Serializable]
    public class PlayerStat
    {
        public int id;
        public Stat stat;
        public float value;
        public string createdAt;
        public string updatedAt;
    }
}
