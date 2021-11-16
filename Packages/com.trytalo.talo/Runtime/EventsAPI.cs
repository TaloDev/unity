using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace TaloGameServices
{
    public class EventsAPI : BaseAPI
    {
        private List<Event> queue = new List<Event>();
        private readonly int minQueueSize = 10;

        public EventsAPI(TaloSettings settings, HttpClient client) : base(settings, client, "events") { }

        public void Track(string name)
        {
            Track(name, null);
        }

        public void Track(string name, params (string, string)[] props)
        {
            Talo.IdentityCheck();

            var ev = new Event();
            ev.aliasId = Talo.CurrentAlias.id;
            ev.name = name;

            if (props != null)
            {
                ev.props = props.Select((propTuple) => new Prop(propTuple)).ToArray();
            }

            ev.timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            queue.Add(ev);

            if (queue.Count >= minQueueSize)
            {
                _ = Flush();
            }
        }

        public async Task Flush()
        {
            Talo.IdentityCheck();

            var eventsToSend = queue.ToArray();
            queue.Clear();

            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(baseUrl);

            string content = JsonUtility.ToJson(new EventsPostRequest(eventsToSend));
            req.Content = new StringContent(content, Encoding.UTF8, "application/json");

            try
            {
                await Call(req);
            }
            catch (HttpRequestException err)
            {
                Debug.LogError(err.Message);
                queue.AddRange(eventsToSend);
            }
        }
    }
}
