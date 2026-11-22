using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaloGameServices
{
    internal class RequestMock
    {
        private struct RequestHandler
        {
            public Uri uri;
            public string method, response;
            public long status;
        }

        internal class RecordedRequest
        {
            public Uri uri;
            public string method, content;
            public List<HttpHeader> headers;

            public string GetHeader(string key)
            {
                return headers?.Find((header) => header.key == key)?.value ?? "";
            }
        }

        private static readonly List<RequestHandler> _permanentHandlers = new();
        private static readonly List<RequestHandler> _oneTimeHandlers = new();
        private static readonly List<RecordedRequest> _requests = new();
        private static bool _offline;

        public static List<RecordedRequest> Requests => _requests;

        public static bool Offline
        {
            get => _offline;
            set => _offline = value;
        }

        public static void Reset()
        {
            _permanentHandlers.Clear();
            _oneTimeHandlers.Clear();
            _requests.Clear();
        }

        private static void AddToHandlerList(List<RequestHandler> list, Uri uri, string method, string response, long status)
        {
            list.Add(new RequestHandler
            {
                uri = uri,
                method = method,
                response = response,
                status = status
            });
        }

        public static void Reply(Uri uri, string method, string response = "", long status = 200)
        {
            AddToHandlerList(_permanentHandlers, uri, method, response, status);
        }

        public static void ReplyOnce(Uri uri, string method, string response = "", long status = 200)
        {
            AddToHandlerList(_oneTimeHandlers, uri, method, response, status);
        }

        private static RequestHandler? FindInHandlerList(List<RequestHandler> list, Uri uri, string method)
        {
            return list
                .Where((m) => m.uri == uri && m.method == method)
                .Cast<RequestHandler?>()
                .FirstOrDefault();
        }

        private static string HandleResult(Uri uri, RequestHandler handler)
        {
            if (handler.status < 400)
            {
                return handler.response;
            }

            // the real client picks the exception type by route
            if (uri.AbsolutePath.Contains("/v1/players/auth/"))
            {
                throw new PlayerAuthException(ErrorCodeOf(handler.response));
            }

            throw new RequestException(handler.status, new Exception(handler.response), handler.response);
        }

        private static string ErrorCodeOf(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return "";
            }

            return JsonUtility.FromJson<ErrorResponse>(response)?.errorCode;
        }

        public static string HandleCall(Uri uri, string method, string content = "", List<HttpHeader> headers = null)
        {
            _requests.Add(new RecordedRequest
            {
                uri = uri,
                method = method,
                content = content,
                headers = headers
            });

            var handler = FindInHandlerList(_permanentHandlers, uri, method);
            if (handler != null)
            {
                return HandleResult(uri, handler.GetValueOrDefault());
            }

            handler = FindInHandlerList(_oneTimeHandlers, uri, method);
            if (handler != null)
            {
                var value = handler.GetValueOrDefault();
                _oneTimeHandlers.Remove(value);
                return HandleResult(uri, value);
            }

            throw new Exception($"Request handler not set for {method} {uri}");
        }
    }
}
