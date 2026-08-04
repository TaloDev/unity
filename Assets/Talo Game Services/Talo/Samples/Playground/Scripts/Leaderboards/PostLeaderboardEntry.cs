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
                var result = await Talo.Leaderboards.AddEntry(leaderboardInternalName, score);

                if (result.Entry == null)
                {
                    ResponseMessage.SetText("Failed to add entry");
                    return;
                }

                ResponseMessage.SetText($"Entry with score {score} added, position is {result.Entry.position}, it was {(result.Updated ? "" : "not")} updated");
            }
            catch (Exception ex)
            {
                ResponseMessage.SetText(ex.Message);
                throw;
            }
        }
    }
}
