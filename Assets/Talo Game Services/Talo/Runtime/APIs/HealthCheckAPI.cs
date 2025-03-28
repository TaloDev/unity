using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class HealthCheckAPI : BaseAPI
    {
        public HealthCheckAPI() : base("v1/health-check") { }

        public async Task<bool> Ping()
        {
            var uri = new Uri(baseUrl);

            try
            {
                await Call(uri, "GET");
                return true;
            } catch
            {
                Debug.LogWarning("Health check failed");
                return false;
            }
        }
    }
}
