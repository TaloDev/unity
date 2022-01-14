using UnityEngine;
using TaloGameServices;
using System;

public class TrackLevelUpEvent : MonoBehaviour
{
    public int level = 1;
    private float timeTaken;

    public void OnButtonClick()
    {
        Track();
    }

    private void Track()
    {
        level++;

        try
        {
            Talo.Events.Track(
                "Level up",
                ("newLevel", $"{level}"),
                ("timeTaken", $"{timeTaken}")
            );

            ResponseMessage.SetText($"Level up tracked, newLevel = {level}, timeTaken = {timeTaken}");
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }

        timeTaken = 0;
    }

    private void Update()
    {
        timeTaken += Time.deltaTime;
    }
}
