using System;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace TaloGameServices
{
    public class BaseAPI
    {
        protected string baseUrl;

        public BaseAPI(string service)
        {
            baseUrl = $"{Talo.Settings.apiUrl}/{service}";
        }

        public Uri GetUri()
        {
            return new Uri(baseUrl);
        }

        private List<HttpHeader> BuildHeaders()
        {
            var headers = new List<HttpHeader>
            {
                new HttpHeader("Authorization", $"Bearer {Talo.Settings.accessKey}"),
                new HttpHeader("Content-Type", "application/json"),
                new HttpHeader("Accept", "application/json"),
                new HttpHeader("X-Talo-Dev-Build", Debug.isDebugBuild ? "1" : "0"),
                new HttpHeader("X-Talo-Include-Dev-Data", Debug.isDebugBuild ? "1" : "0")
            };

            if (Talo.CurrentAlias != null)
            {
                headers.Add(new HttpHeader("X-Talo-Player", Talo.CurrentPlayer.id));
                headers.Add(new HttpHeader("X-Talo-Alias", Talo.CurrentAlias.id.ToString()));
            }

            var sessionToken = Talo.PlayerAuth.SessionManager.GetSessionToken();
            if (!string.IsNullOrEmpty(sessionToken))
            {
                headers.Add(new HttpHeader("X-Talo-Session", sessionToken));
            }

            return headers;
        }

        protected async Task<string> Call(
            Uri uri,
            string method,
            string content = "",
            List<HttpHeader> headers = null,
            bool continuity = false
        )
        {
            if (Talo.TestMode)
            {
                return RequestMock.HandleCall(uri, method);
            }

            var continuityTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var allHeaders = continuity ? headers : BuildHeaders();

            if (Talo.Settings.offlineMode)
            {
                return HandleOfflineRequest(uri, method, content, allHeaders);
            }

            byte[] json = new System.Text.UTF8Encoding().GetBytes(content);

            using (UnityWebRequest www = new(uri, method))
            {
                if (json.Length > 0) www.uploadHandler = new UploadHandlerRaw(json);
                www.downloadHandler = new DownloadHandlerBuffer();

                foreach (var header in allHeaders)
                {
                    www.SetRequestHeader(header.key, header.value);
                }

                var op = www.SendWebRequest();

                while (!op.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    return www.downloadHandler.text;
                }
                else
                {
                    if (www.responseCode >= 500 || www.result != UnityWebRequest.Result.ProtocolError)
                    {
                        Talo.Continuity.PushRequest(uri, method, content, headers, continuityTimestamp);
                    }

                    string message = "";
                    string errorCode = "";

                    try
                    {
                        var jsonError = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
                        message = string.IsNullOrEmpty(jsonError.message) ? www.downloadHandler.text : jsonError.message;
                        errorCode = jsonError.errorCode;
                    }
                    catch (Exception)
                    {
                        message = www.error;
                    }

                    if (string.IsNullOrEmpty(errorCode))
                    {
                        throw new RequestException(www.responseCode, new Exception(message));
                    }
                    else
                    {
                        throw new PlayerAuthException(errorCode, new Exception(message));
                    }
                }
            }
        }

        private string HandleOfflineRequest(
            Uri uri,
            string method,
            string content = "",
            List<HttpHeader> headers = null
        )
        {
            Talo.Continuity.PushRequest(uri, method, content, headers, DateTimeOffset.Now.ToUnixTimeMilliseconds());
            throw new RequestException(0, new Exception("Offline mode enabled"));
        }
    }
}
