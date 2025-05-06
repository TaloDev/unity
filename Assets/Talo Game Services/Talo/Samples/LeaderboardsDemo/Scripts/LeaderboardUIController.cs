using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace TaloGameServices.Sample.LeaderboardsDemo
{
    public class LeaderboardUIController : MonoBehaviour
    {
        public string leaderboardName;
        public bool includeArchived;

        private VisualElement root;
        private ListView entriesList;
        private Label infoLabel;

        private int filterIdx;
        private string filter = "All";

        private async void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            root.Q<Button>("post-btn").clicked += OnPostClick;
            root.Q<Button>("filter-btn").clicked += OnFilterClick;

            entriesList = root.Q<ListView>();
            infoLabel = root.Q<Label>("info");

            if (string.IsNullOrEmpty(leaderboardName))
            {
                throw new Exception("Please create a leaderboard and set the leaderboard name to its internal name");
            }

            await LoadEntries();
        }

        private async void OnPostClick()
        {
            var username = root.Q<TextField>().text;
            var score = UnityEngine.Random.Range(0, 100);
            var team = UnityEngine.Random.Range(0, 2) == 0 ? "Blue" : "Red";

            await Talo.Players.Identify("username", username);
            (LeaderboardEntry entry, bool updated) = await Talo.Leaderboards.AddEntry(
                leaderboardName,
                score,
                ("team", team)
            );

            infoLabel.text = $"You scored {score} for the {team} team.";
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
                try
                {
                    var res = await Talo.Leaderboards.GetEntries(leaderboardName, new GetEntriesOptions() {
                        page = page,
                        includeArchived = includeArchived
                    });

                    page++;
                    done = res.isLastPage;
                }
                catch (RequestException e)
                {
                    if (e.responseCode == 404)
                    {
                        infoLabel.text = $"Failed loading leaderboard {leaderboardName}. Does it exist?";
                    }
                    else
                    {
                        infoLabel.text = e.Message;
                        Debug.LogError(e);
                    }
                    return;
                }

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
                LeaderboardEntry entry = entriesList.itemsSource[i] as LeaderboardEntry;
                var teamText = entry.GetProp("team", "No");
                var archivedText = !string.IsNullOrEmpty(entry.deletedAt) ? " (archived)" : "";
                e.Q<Label>().text = $"{i+1}. {entry.playerAlias.identifier} - {entry.score} ({teamText} team){archivedText}";
            };

            entriesList.itemsSource = Talo.Leaderboards.GetCachedEntries(leaderboardName);
        }

        private string GetNextFilter(int idx)
        {
            return new [] { "All", "Blue", "Red" } [idx % 3];
        }

        private void OnFilterClick()
        {
            filterIdx++;
            filter = GetNextFilter(filterIdx);

            infoLabel.text = $"Filtering on {filter.ToLower()}";
            root.Q<Button>("filter-btn").text = $"{GetNextFilter(filterIdx + 1)} team scores";

            if (filter == "All")
            {
                entriesList.itemsSource = Talo.Leaderboards.GetCachedEntries(leaderboardName);
            }
            else
            {
                entriesList.itemsSource = new List<LeaderboardEntry>(Talo.Leaderboards.GetCachedEntries(leaderboardName)
                    .FindAll((e) => e.GetProp("team", "") == filter));
            }

            entriesList.Rebuild();
        }
    }
}
