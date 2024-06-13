using System;
using System.Collections.Generic;
using System.Linq;

internal class RequestMock
{
    private struct RequestHandler
    {
        public Uri uri;
        public string method, response;
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

    private static void AddToHandlerList(List<RequestHandler> list, Uri uri, string method, string response)
    {
        list.Add(new RequestHandler
        {
            uri = uri,
            method = method,
            response = response
        });
    }

    public static void Reply(Uri uri, string method, string response = "")
    {
        AddToHandlerList(_permanentHandlers, uri, method, response);
    }

    public static void ReplyOnce(Uri uri, string method, string response = "")
    {
        AddToHandlerList(_oneTimeHandlers, uri, method, response);
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
            return handler.GetValueOrDefault().response;
        }
        else
        {
            handler = FindInHandlerList(_oneTimeHandlers, uri, method);
            if (handler != null)
            {
                _oneTimeHandlers.Remove(handler.GetValueOrDefault());
                return handler.GetValueOrDefault().response;
            }
            else
            {
                throw new Exception($"Request handler not set for {method} {uri}");
            }
        }
    }
}
