using UnityEngine;

namespace TaloGameServices
{
    public class TaloManager : MonoBehaviour
    {
        public TaloSettings settings;

        private float tmrFlush;
        private float tmrContinuity;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnReady()
        {
            Talo.Events.OnFlushed += ResetFlushTimer;
        }

        private void OnDisable()
        {
            Talo.Events.OnFlushed -= ResetFlushTimer;
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

            if (Talo.Continuity.HasRequests())
            {
                tmrContinuity += Time.deltaTime;
                if (tmrContinuity >= 10f)
                {
                    Talo.Continuity.ProcessRequests();
                    tmrContinuity = 0;
                }
            }
        }

        public void ResetFlushTimer()
        {
            tmrFlush = 0;
        }
    }
}
