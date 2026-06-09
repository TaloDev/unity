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

        [Tooltip("Enable request logs in Debug builds")]
        public bool logRequests = true;

        [Tooltip("Enable response logs in Debug builds")]
        public bool logResponses = true;

        [Tooltip("If a valid session token is found, automatically authenticate the player")]
        public bool autoStartSession = true;

        [Tooltip("If enabled, Talo will automatically cache the player after a successful online identification for use in later offline sessions")]
        public bool cachePlayerOnIdentify = true;

        [Tooltip("Number of seconds to wait before sending debounced requests (e.g. player updates, save updates and health checks)")]
        public float debounceTimerSeconds = 1f;

        [Tooltip("Enable request verification to prevent replay attacks and tampering - this must also be enabled in the dashboard")]
        public bool verificationEnabled = false;

        [Tooltip("The version of the verification key being used")]
        public string verificationKeyVersion = "";

        [Tooltip("The value for the verification key version")]
        public string verificationKeyValue = "";
    }
}
