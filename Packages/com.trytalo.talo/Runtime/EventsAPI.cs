using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaloGameServices
{
    public class EventsAPI : BaseAPI
    {
        private List<Event> queue = new List<Event>();
        private readonly int minQueueSize = 10;

        public EventsAPI(TaloManager manager) : base(manager, "events") { }

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
            
            if (eventsToSend.Length > 0)
            {
                queue.Clear();

                var uri = new Uri(baseUrl);
                var content = JsonUtility.ToJson(new EventsPostRequest(eventsToSend));

                try
                {
                    await Call(uri, "POST", content);
                    manager.ResetFlushTimer();
                }
                catch (Exception err)
                {
                    Debug.LogError(err.Message);
                    queue.AddRange(eventsToSend);
                }
            }
        }
    }
}
