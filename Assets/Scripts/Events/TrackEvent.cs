using System;
using UnityEngine;
using TaloGameServices;
using System.Linq;

public class TrackEvent : MonoBehaviour
{
    public string eventName;
    public Prop[] props;
    public bool flushImmediately;

    public void OnButtonClick()
    {
        Track();
    }

    private void Track()
    {
        try
        {
            Talo.Events.Track(eventName, props.Select((prop) => (prop.key, prop.value)).ToArray());

            ResponseMessage.SetText($"{eventName} tracked");

            if (flushImmediately)
            {
                Talo.Events.Flush();

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
