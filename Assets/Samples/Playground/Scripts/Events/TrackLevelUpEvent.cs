using UnityEngine;
using TaloGameServices;
using System;
using System.Threading.Tasks;

public class TrackLevelUpEvent : MonoBehaviour
{
    public int level = 1;
    private float timeTaken;

    public async void OnButtonClick()
    {
        await Track();
    }

    private async Task Track()
    {
        level++;

        try
        {
            await Talo.Events.Track(
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
