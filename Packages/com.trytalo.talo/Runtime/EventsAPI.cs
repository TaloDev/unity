using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System;
using System.Text;
using System.Threading.Tasks;

namespace TaloGameServices {
    public class EventsAPI : BaseAPI {
        private List<Event> queue = new List<Event>();

        public EventsAPI(TaloSettings settings, HttpClient client) : base(settings, client, "events") { }

        public async void Track(string name, Prop[] props) {
            Talo.IdentityCheck();

            var ev = new Event();
            ev.aliasId = Talo.CurrentPlayer.id;
            ev.name = name;
            ev.props = props;
            ev.timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            queue.Add(ev);

            Debug.Log(queue.Count);

            if (queue.Count >= settings.eventQueueSize) {
                await Task.Run(Flush);
            }
        }

        public async void Flush() {
            Talo.IdentityCheck();

            var eventsToSend = queue.ToArray();
            queue.Clear();

            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(baseUrl);

            string content = JsonUtility.ToJson(new EventsPostRequest(eventsToSend));
            req.Content = new StringContent(content, Encoding.UTF8, "application/json");

            try {
                await Call(req);
            } catch (HttpRequestException err) {
                Debug.LogError(err.Message);
                queue.AddRange(eventsToSend);
            }
        }
    }
}
