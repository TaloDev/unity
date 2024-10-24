using System;
using TaloGameServices;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class GetLeaderboardEntries : MonoBehaviour
{
    public string leaderboardInternalName;
    public int page = 0;

    public async void OnButtonClick()
    {
        await FetchEntries();
    }

    private async Task FetchEntries()
    {
        if (string.IsNullOrEmpty(leaderboardInternalName))
        {
            ResponseMessage.SetText("leaderboardInternalName not set on GetLeaderboardEntriesButton");
            return;
        }

        try
        {
            int score = UnityEngine.Random.Range(0, 10000);
            LeaderboardEntriesResponse res = await Talo.Leaderboards.GetEntries(leaderboardInternalName, page);
            LeaderboardEntry[] entries = res.entries;

            if (entries.Length == 0)
            {
                ResponseMessage.SetText($"No entries for page {page}");
            }
            else
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
