using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaloGameServices
{
    public class EventsAPI : BaseAPI
    {
        public event Action OnFlushed;

        internal List<Event> queue = new ();
        private readonly int minQueueSize = 10;

        internal List<Event> eventsToFlush = new ();
        private bool lockFlushes;
        private bool flushAttemptedDuringLock;

        public EventsAPI() : base("v1/events") { }

        private string GetWindowMode()
        {
            if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
            {
                return "Exclusive fullscreen";
            }
            else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                return "Fullscreen window";
            }
            else if (Screen.fullScreenMode == FullScreenMode.MaximizedWindow)
            {
                return "Maximized window";
            }
            else if (Screen.fullScreenMode == FullScreenMode.Windowed)
            {
                return "Windowed";
            }

            return "";
        }

        private Prop[] BuildMetaProps()
        {
            return new Prop[]
                {
                    new Prop(("META_OS", SystemInfo.operatingSystem)),
                    new Prop(("META_GAME_VERSION", Application.version)),
                    new Prop(("META_WINDOW_MODE", GetWindowMode())),
                    new Prop(("META_SCREEN_WIDTH", Screen.width.ToString())),
                    new Prop(("META_SCREEN_HEIGHT", Screen.height.ToString()))
                };
        }

        public async Task Track(string name, params (string, string)[] props)
        {
            Talo.IdentityCheck();

            var ev = new Event
            {
                name = name
            };

            ev.props = props
                .Select((propTuple) => new Prop(propTuple))
                .Concat(BuildMetaProps())
                .ToArray();

            ev.timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            queue.Add(ev);

            if (queue.Count >= minQueueSize)
            {
                await Flush();
            }
        }

        public async Task Flush()
        {
            Talo.IdentityCheck();

            var eventsToSend = queue.ToArray();
            if (eventsToSend.Length == 0)
            {
                return;
            }

            if (lockFlushes)
            {
                flushAttemptedDuringLock = true;
                return;
            }

            var uri = new Uri(baseUrl);
            var content = JsonUtility.ToJson(new EventsPostRequest(eventsToSend));

            try
            {
                lockFlushes = true;
                eventsToFlush.AddRange(eventsToSend);
                queue.Clear();

                await Call(uri, "POST", content);
                OnFlushed.Invoke();

                eventsToFlush.Clear();
                lockFlushes = false;
            }
            catch (Exception err)
            {
                Debug.LogError(err.Message);
            }
            
            if (flushAttemptedDuringLock)
            {
                flushAttemptedDuringLock = false;
                await Flush();
            }
        }

        public void ClearQueue()
        {
            queue.Clear();
            eventsToFlush.Clear();
            lockFlushes = false;
            flushAttemptedDuringLock = false;
        }
    }
}
