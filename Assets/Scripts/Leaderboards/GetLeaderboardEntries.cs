using System;
using TaloGameServices;
using UnityEngine;
using System.Linq;

public class GetLeaderboardEntries : MonoBehaviour
{
    public string internalName;
    public int page;

    public void OnButtonClick()
    {
        FetchEntries();
    }

    private async void FetchEntries()
    {
        try
        {
            int score = UnityEngine.Random.Range(0, 10000);
            LeaderboardEntry[] entries = await Talo.Leaderboards.GetEntries(internalName, page);

            if (entries.Length == 0)
            {
                ResponseMessage.SetText($"No entries for page {page}");
            } else
            {
                ResponseMessage.SetText(string.Join(", ", entries.Select((e) => e.ToString()).ToArray()));
            }
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
