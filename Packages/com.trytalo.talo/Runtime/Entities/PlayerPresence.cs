using System;

namespace TaloGameServices
{
    [Serializable]
    public class PlayerPresence
    {
        public bool online;
        public string customStatus;
        public PlayerAlias playerAlias;
        public string updatedAt;
    }
}
