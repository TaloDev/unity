using UnityEngine;

namespace TaloGameServices
{
    [CreateAssetMenu(fileName = "Talo Settings", menuName = "Talo/Settings Asset", order = 1)]
    public class TaloSettings : ScriptableObject
    {
        public string accessKey = "";
        public string apiUrl = "https://api.trytalo.com";
        public string socketUrl = "wss://api.trytalo.com";
        [Tooltip("How often in seconds events are flushed in a WebGL build, see the docs for more info")]
        public float webGLEventFlushRate = 30f;
        [Tooltip("Replays network requests that failed due to network issues")]
        public bool continuityEnabled = true;
        [Tooltip("Simulates being offline, useful for testing")]
        public bool offlineMode = false;
        [Tooltip("Whether to automatically connect to the Talo socket when the game starts")]
        public bool autoConnectSocket = true;
    }
}
