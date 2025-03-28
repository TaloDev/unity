using System;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace TaloGameServices.Sample.Playground
{
    public class TrackEvent : MonoBehaviour
    {
        public string eventName;
        public Prop[] props;
        public bool flushImmediately;

        public async void OnButtonClick()
        {
            await Track();
        }

        private async Task Track()
        {
            if (string.IsNullOrEmpty(eventName))
            {
                ResponseMessage.SetText("eventName not set on TrackEventButton");
                return;
            }

            try
            {
                await Talo.Events.Track(eventName, props.Select((prop) => (prop.key, prop.value)).ToArray());

                ResponseMessage.SetText($"{eventName} tracked");

                if (flushImmediately)
                {
                    await Talo.Events.Flush();

                    ResponseMessage.SetText($"{eventName} tracked and events flushed");
                }
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
