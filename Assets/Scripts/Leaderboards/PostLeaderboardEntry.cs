using System;
using TaloGameServices;
using UnityEngine;

public class PostLeaderboardEntry : MonoBehaviour
{
    public string internalName;

    public void OnButtonClick()
    {
        PostEntry();
    }

    private async void PostEntry()
    {
        try
        {
            int score = UnityEngine.Random.Range(0, 10000);
            (LeaderboardEntry entry, bool updated) = await Talo.Leaderboards.AddEntry(internalName, score);

            ResponseMessage.SetText($"Entry with score {score} added, position is {entry.position}, it was {(updated ? "" : "not")} updated");
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
