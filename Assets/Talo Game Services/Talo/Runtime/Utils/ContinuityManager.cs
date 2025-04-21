using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

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
        public List<Request> requests = new List<Request>();
    }

    public class ContinuityManager
    {
        private readonly string _continuityPath = Application.persistentDataPath + "/tc.bin";

        private List<Request> _requests;

        private ContinuityAPI _api = new ContinuityAPI();

        private string[] _excludedEndpoints = {
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

        public bool HasRequests()
        {
            return _requests.Count > 0;
        }

        public void PushRequest(Uri uri, string method, string content, List<HttpHeader> headers, long timestamp)
        {
            if (!Talo.Settings.continuityEnabled) return;

            if (Array.Exists(_excludedEndpoints, (e) => uri.AbsolutePath.Contains(e))) return;

            _requests.Add(new Request
            {
                uri = uri.ToString(),
                method = method,
                content = content,
                headers = (headers ?? new List<HttpHeader>()).Where((h) => h.key != "Authorization").ToList(),
                timestamp = timestamp
            });

            WriteRequests();
        }

        public async void ProcessRequests()
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
                    new HttpHeader("Authorization", $"Bearer {Talo.Settings.accessKey}")
                }).ToList();

                if (request.headers.Find((h) => h.key == "X-Talo-Continuity-Timestamp") == null)
                {
                    headers.Add(new HttpHeader("X-Talo-Continuity-Timestamp", request.timestamp.ToString()));
                }

                try
                {
                    await _api.Replay(uri, request.method, request.content, headers);
                } catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new ContinuityReplayException(exceptions);
            }
        }
    }
}
