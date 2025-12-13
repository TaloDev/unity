using System;
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
            Talo.Saves.Setup();
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
            try
            {
                if (Talo.HasIdentity())
                {
                    await Talo.Events.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to flush events: {ex}");
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
                    ProcessContinuityRequests();
                    tmrContinuity = 0;
                }
            }

            ProcessDebouncedUpdates();
        }

        private async void ProcessContinuityRequests()
        {
            try
            {
                await Talo.Continuity.ProcessRequests();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process continuity requests: {ex}");
            }
        }

        private async void ProcessDebouncedUpdates()
        {
            try
            {
                if (Talo.HasIdentity())
                {
                    await Talo.Players.ProcessPendingUpdates();
                }

                if (Talo.Saves.Current != null)
                {
                    await Talo.Saves.ProcessPendingUpdates();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process debounced updates: {ex}");
            }
        }

        public void ResetFlushTimer()
        {
            tmrFlush = 0;
        }
    }
}
