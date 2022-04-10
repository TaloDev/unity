using UnityEngine;

namespace TaloGameServices
{
    public class TaloManager : MonoBehaviour
    {
        public TaloSettings settings;

        private float tmrFlush;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            DoFlush();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) DoFlush();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused) DoFlush();
        }

        private async void DoFlush()
        {
            if (Talo.HasIdentity())
            {
                await Talo.Events.Flush();
            }
        }

        private void Update()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                tmrFlush += Time.deltaTime;
                if (tmrFlush >= settings.webGLEventFlushRate)
                {
                    DoFlush();
                    tmrFlush = 0;
                }
            }
        }

        public void ResetFlushTimer()
        {
            tmrFlush = 0;
        }
    }
}
