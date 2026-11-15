using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TaloGameServices
{
    [Serializable]
    public struct Request
    {
        public string uri;
        public string method;
        public string content;
        public List<HttpHeader> headers;
        public long timestamp;
    }

    [Serializable]
    public class ContinuityContent
    {
        public List<Request> requests = new();
    }

    public class ContinuityManager
    {
        internal const string AuthorizationHeader = "Authorization";
        internal const string SignatureHeader = "X-Talo-Signature";
        internal const string ContinuityTimestampHeader = "X-Talo-Continuity-Timestamp";

        // ProcessRequests writes these headers on every retry.
        // PushRequest strips them so the queue keeps one copy and the first continuity timestamp wins.
        private static readonly string[] _replayBuiltHeaders = {
            AuthorizationHeader,
            SignatureHeader,
            ContinuityTimestampHeader
        };

        private readonly string _continuityPath = Application.persistentDataPath + "/tc.bin";

        private readonly List<Request> _requests;

        private readonly ContinuityAPI _api = new();

        private readonly string[] _excludedEndpoints = {
            "/health-check",
            "/players/auth",
            "/players/identify",
            "/socket-tickets"
        };

        public ContinuityManager()
        {
            _requests = new List<Request>(ReadRequests().requests);
        }

        private ContinuityContent ReadRequests()
        {
            if (!File.Exists(_continuityPath) || Talo.TestMode) return new ContinuityContent();
            return JsonUtility.FromJson<ContinuityContent>(Talo.Crypto.ReadFileContent(_continuityPath));
        }

        private void WriteRequests()
        {
            var content = JsonUtility.ToJson(new ContinuityContent { requests = _requests });
            Talo.Crypto.WriteFileContent(_continuityPath, content);
        }

        internal IReadOnlyList<Request> Requests => _requests;

        public bool HasRequests()
        {
            return _requests.Count > 0;
        }

        public void PushRequest(Uri uri, string method, string content, List<HttpHeader> headers, long timestamp)
        {
            if (!Talo.Settings.continuityEnabled) return;
            if (Array.Exists(_excludedEndpoints, (e) => uri.AbsolutePath.Contains(e))) return;

            var allowedMethods = new[] { "POST", "PUT", "PATCH", "DELETE" };
            if (!Array.Exists(allowedMethods, (m) => m == method)) return;

            _requests.Add(new Request
            {
                uri = uri.ToString(),
                method = method,
                content = content,
                headers = (headers ?? new List<HttpHeader>()).Where((h) => !Array.Exists(_replayBuiltHeaders, (k) => k == h.key)).ToList(),
                timestamp = ReadContinuityTimestamp(headers) ?? timestamp
            });

            WriteRequests();
        }

        // a request that fails again during replay keeps the timestamp it was first queued with
        private static long? ReadContinuityTimestamp(List<HttpHeader> headers)
        {
            var header = headers?.Find((h) => h.key == ContinuityTimestampHeader);
            if (header == null)
            {
                return null;
            }

            if (!long.TryParse(header.value, out var value))
            {
                return null;
            }

            return value;
        }

        public async Task ProcessRequests()
        {
            if (!HasRequests() || !await Talo.HealthCheck.Ping()) return;

            var queue = new List<Request>(_requests.Take(10));
            var exceptions = new List<Exception>();

            for (var i = 0; i < queue.Count; i++)
            {
                var request = queue[i];
                _requests.RemoveAt(0);
                WriteRequests();

                var uri = new Uri(request.uri);
                var headers = request.headers.Concat(new List<HttpHeader> {
                    new(AuthorizationHeader, $"Bearer {Talo.Settings.accessKey}")
                }).ToList();

                if (request.headers.Find((h) => h.key == ContinuityTimestampHeader) == null)
                {
                    headers.Add(new HttpHeader(ContinuityTimestampHeader, request.timestamp.ToString()));
                }

                if (Talo.Settings.verificationEnabled)
                {
                    headers.Add(new HttpHeader(SignatureHeader, CryptoManager.CreateRequestSignature(request.content)));
                }

                try
                {
                    await _api.Replay(uri, request.method, request.content, headers);
                } catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new ContinuityReplayException(exceptions);
            }
        }

        public void ClearRequests()
        {
            _requests.Clear();
            WriteRequests();
        }

        public async Task HandlePostResponseHealthCheck(string url, UnityWebRequest.Result result)
        {
            if (url.Contains("/health-check"))
            {
                return;
            }

            var isConnectionError = result == UnityWebRequest.Result.ConnectionError;
            var isDataProcessingError = result == UnityWebRequest.Result.DataProcessingError;

            if (result == UnityWebRequest.Result.Success)
            {
                // if offline mode is enabled, check if it shouldn't be
                if (Talo.HealthCheck.GetLastStatus() == HealthCheckStatus.FAILED)
                {
                    await Talo.HealthCheck.Ping();
                }
            }
            else if (isConnectionError || isDataProcessingError)
            {
                // if offline mode isn't enabled, check if it should be
                if (Talo.HealthCheck.GetLastStatus() != HealthCheckStatus.FAILED)
                {
                    await Talo.HealthCheck.Ping();
                }
            }
        }
    }
}
