using UnityEngine;

namespace TaloGameServices {
    [CreateAssetMenu(fileName = "Talo Settings", menuName = "Talo/Settings Asset", order = 1)]
    public class TaloSettings : ScriptableObject {
        public string apiKey;
        public int eventQueueSize = 10;
    }
}
