using UnityEngine;

namespace TaloGameServices
{
    public class TaloManager : MonoBehaviour
    {
        public TaloSettings settings;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private async void OnApplicationQuit()
        {
            await Talo.Events.Flush();
        }
    }
}
