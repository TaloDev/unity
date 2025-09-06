using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace TaloGameServices.Sample.Playground
{
    public class SearchPlayers : MonoBehaviour
    {
        public InputField inputField;

        public async void OnButtonClick()
        {
            var query = inputField.text;
            if (string.IsNullOrWhiteSpace(query))
            {
                ResponseMessage.SetText("Please enter a query e.g. a player ID, alias identifier or prop value");
                return;
            }

            ResponseMessage.SetText($"Searching for {query}...");
            var res = await Talo.Players.Search(query);
            if (res.count == 0)
            {
                ResponseMessage.SetText("No players found");
                return;
            }

            var identifiers = res.players.Select((player) => player.GetAlias());
            ResponseMessage.SetText($"Found {res.count} results: {string.Join(", ", identifiers)}");
        }
    }
}
