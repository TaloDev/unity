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
    private List<LeaderboardEntry> entries = new ();

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

        var idx = entries.FindIndex((existingEntry) => existingEntry.playerAlias.id == entry.playerAlias.id);
        if (idx != -1)
        {
            entries.RemoveAt(idx);
        }

        entries.Insert(entry.position, entry);

        infoLabel.text = $"You scored {score}.";
        if (updated) infoLabel.text += " Your highscore was updated!";

        entriesList.Rebuild();
    }

    private void HandleListVisibility()
    {
        if (entries.Count == 0)
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
            entries.AddRange(res.entries);

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

        entriesList.bindItem = (e, i) =>
        {
            e.Q<Label>().text = $"{i+1}. {entries[i].playerAlias.identifier} - {entries[i].score}";
        };

        entriesList.itemsSource = entries;
    }
}
