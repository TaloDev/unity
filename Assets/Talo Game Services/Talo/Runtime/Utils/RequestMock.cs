using System;
using System.Collections.Generic;
using System.Linq;

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

        private static List<RequestHandler> _permanentHandlers = new List<RequestHandler>();
        private static List<RequestHandler> _oneTimeHandlers = new List<RequestHandler>();
        private static bool _offline;

        public static bool Offline
        {
            get => _offline;
            set => _offline = value;
        }

        public static void Reset()
        {
            _permanentHandlers.Clear();
            _oneTimeHandlers.Clear();
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

        public static string HandleCall(Uri uri, string method)
        {
            var handler = FindInHandlerList(_permanentHandlers, uri, method);
            if (handler != null)
            {
                var value = handler.GetValueOrDefault();
                return value.response;
            }
            else
            {
                handler = FindInHandlerList(_oneTimeHandlers, uri, method);
                if (handler != null)
                {
                    var value = handler.GetValueOrDefault();
                    _oneTimeHandlers.Remove(value);
                    return value.response;
                }
                else
                {
                    throw new Exception($"Request handler not set for {method} {uri}");
                }
            }
        }
    }
}
