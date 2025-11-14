using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public enum HealthCheckStatus
    {
        OK,
        FAILED,
        UNKNOWN
    }

    public class HealthCheckAPI : BaseAPI
    {
        private HealthCheckStatus lastHealthCheckStatus = HealthCheckStatus.UNKNOWN;
        private float nextPingTime;

        public HealthCheckAPI() : base("v1/health-check") { }

        public HealthCheckStatus GetLastStatus()
        {
            return lastHealthCheckStatus;
        }

        public async Task<bool> Ping()
        {
            var bustCache = lastHealthCheckStatus == HealthCheckStatus.UNKNOWN || Time.realtimeSinceStartup >= nextPingTime;
            if (!bustCache)
            {
                return lastHealthCheckStatus == HealthCheckStatus.OK;
            }

            var uri = new Uri(baseUrl);

            bool success;
            try
            {
                await Call(uri, "GET");
                success = true;
            }
            catch
            {
                Debug.LogWarning("Health check failed");
                success = false;
            }

            bool failedLastHealthCheck = lastHealthCheckStatus == HealthCheckStatus.FAILED;

            if (success)
            {
                lastHealthCheckStatus = HealthCheckStatus.OK;
                if (failedLastHealthCheck)
                {
                    Talo.InvokeConnectionRestored();
                }
            }
            else
            {
                lastHealthCheckStatus = HealthCheckStatus.FAILED;
                if (!failedLastHealthCheck)
                {
                    Talo.InvokeConnectionLost();
                }
            }

            nextPingTime = Time.realtimeSinceStartup + Talo.Settings.debounceTimerSeconds;

            return success;
        }
    }
}
