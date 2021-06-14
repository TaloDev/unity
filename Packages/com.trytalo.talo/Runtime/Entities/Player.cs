using UnityEngine;

namespace TaloGameServices {
    [System.Serializable]
    public class Player {
        public string id;
        public Prop[] props;
        public PlayerAlias[] aliases;
        public string createdAt;
        public string lastSeenAt;

        public override string ToString() {
            return JsonUtility.ToJson(this);
        }
    }
}
