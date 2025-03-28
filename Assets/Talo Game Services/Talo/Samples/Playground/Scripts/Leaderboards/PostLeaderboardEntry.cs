using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices.Sample.Playground
{
    public class PostLeaderboardEntry : MonoBehaviour
    {
        public string leaderboardInternalName;

        public async void OnButtonClick()
        {
            await PostEntry();
        }

        private async Task PostEntry()
        {
            if (string.IsNullOrEmpty(leaderboardInternalName))
            {
                ResponseMessage.SetText("leaderboardInternalName not set on AddEntryButton");
                return;
            }

            try
            {
                int score = UnityEngine.Random.Range(0, 10000);
                (LeaderboardEntry entry, bool updated) = await Talo.Leaderboards.AddEntry(leaderboardInternalName, score);

                ResponseMessage.SetText($"Entry with score {score} added, position is {entry.position}, it was {(updated ? "" : "not")} updated");
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
