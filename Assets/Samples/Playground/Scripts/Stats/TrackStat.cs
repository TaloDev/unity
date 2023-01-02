using System;
using UnityEngine;
using TaloGameServices;

public class TrackStat : MonoBehaviour
{
    public string statInternalName;
    public float change = 1;

    public void OnButtonClick()
    {
        Track();
    }

    private void Track()
    {
        try
        {
            Talo.Stats.Track(statInternalName, change);

            ResponseMessage.SetText($"{statInternalName} changed by {change}");
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
