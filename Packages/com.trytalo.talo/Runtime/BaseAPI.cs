using System;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

namespace TaloGameServices
{
    public class BaseAPI
    {
        protected TaloSettings settings;
        protected string baseUrl;
        private readonly HttpClient client;

        public BaseAPI(TaloSettings settings, HttpClient client, string service)
        {
            this.settings = settings;
            this.client = client;
            baseUrl = $"{settings.apiUrl}/{service}";
        }

        protected async Task<string> Call(HttpRequestMessage req)
        {
            try
            {
                req.Headers.Add("Authorization", $"Bearer {settings.accessKey}");
                var res = await client.SendAsync(req);
                string body = await res.Content.ReadAsStringAsync();

                if (res.IsSuccessStatusCode)
                {
                    return body;
                }
                else
                {
                    string message;

                    try
                    {
                        message = JsonUtility.FromJson<ErrorResponse>(body).message;
                    } catch (Exception)
                    {
                        message = body;
                    }
                    throw new Exception($"Request failed, {(int)res.StatusCode} {res.StatusCode}: {message}");
                }
            }
            catch (HttpRequestException err)
            {
                throw err;
            }
        }
    }
}
