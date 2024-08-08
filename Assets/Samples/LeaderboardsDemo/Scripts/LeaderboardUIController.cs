using UnityEngine;
using TaloGameServices;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class LeaderboardUIController : MonoBehaviour
{
    public string leaderboardName;

    private VisualElement root;
    private ListView entriesList;
    private Label infoLabel;

    private async void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("post-btn").clicked += OnPostClick;

        entriesList = root.Q<ListView>();
        infoLabel = root.Q<Label>("info");

        if (string.IsNullOrEmpty(leaderboardName))
        {
            throw new System.Exception("Please create a leaderboard and set the leaderboard name to its internal name");
        }

        await LoadEntries();
    }

    private async void OnPostClick()
    {
        var username = root.Q<TextField>().text;
        await Talo.Players.Identify("username", username);

        var score = Random.Range(0, 100);
        (LeaderboardEntry entry, bool updated) = await Talo.Leaderboards.AddEntry(leaderboardName, score);

        infoLabel.text = $"You scored {score}.";
        if (updated) infoLabel.text += " Your highscore was updated!";

        entriesList.Rebuild();
    }

    private void HandleListVisibility()
    {
        if (Talo.Leaderboards.GetCachedEntries(leaderboardName).Count == 0)
        {
            infoLabel.text = "There are currently no entries";
        }
        else
        {
            infoLabel.text = "";
        }
    }

    private async Task LoadEntries()
    {
        var page = 0;
        var done = false;

        do
        {
            var res = await Talo.Leaderboards.GetEntries(leaderboardName, page);
            page++;
            done = res.isLastPage;
        } while (!done);
        
        HandleListVisibility();

        entriesList.makeItem = () =>
        {
            var label = new Label();
            label.style.color = new StyleColor(Color.white);
            label.style.fontSize = 22;

            return label;
        };

        var entries = Talo.Leaderboards.GetCachedEntries(leaderboardName);

        entriesList.bindItem = (e, i) =>
        {
            e.Q<Label>().text = $"{i+1}. {entries[i].playerAlias.identifier} - {entries[i].score}";
        };

        entriesList.itemsSource = entries;
    }
}
