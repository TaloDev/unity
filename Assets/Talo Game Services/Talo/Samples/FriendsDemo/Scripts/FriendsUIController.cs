using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

namespace TaloGameServices.Sample.FriendsDemo
{
    [RequireComponent(typeof(UIDocument))]
    public class FriendsUIController : MonoBehaviour
    {
        public const string LookingForFriendsStatus = "Looking for friends!";
        private static readonly string[] RandomNames = { "Alice", "Bob", "Carl", "Doric", "Eve", "Fred", "Gina", "Hank" };

        private string playerName;
        private PlayersManager playersManager;
        private SubscriptionsManager subscriptionsManager;
        private UIManager uiManager;

        private Label playerNameLabel;
        private TextField broadcastInput;

        private async void Start()
        {
            InitializeManagers();
            ConnectEvents();
            await IdentifyPlayer();
            await FindOnlinePlayers();
        }

        private void InitializeManagers()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            // UI elements
            playerNameLabel = root.Q<Label>("player-name");
            var feedScrollView = root.Q<ScrollView>("activity-feed");
            var playersScrollView = root.Q<ScrollView>("online-players");
            var friendsScrollView = root.Q<ScrollView>("friends-list");
            var pendingRequestsScrollView = root.Q<ScrollView>("incoming-requests");
            var outgoingRequestsScrollView = root.Q<ScrollView>("outgoing-requests");
            broadcastInput = root.Q<TextField>("broadcast-input");

            var broadcastButton = root.Q<Button>("broadcast-btn");
            broadcastButton.clicked += OnSendBroadcastPressed;

            // managers
            playersManager = new PlayersManager();
            subscriptionsManager = new SubscriptionsManager();
            uiManager = new UIManager(
                feedScrollView,
                playersScrollView,
                friendsScrollView,
                pendingRequestsScrollView,
                outgoingRequestsScrollView
            );

            // manager events
            playersManager.PlayersUpdated += OnPlayersUpdated;
            subscriptionsManager.FriendsUpdated += OnFriendsUpdated;
            subscriptionsManager.PendingRequestsUpdated += OnPendingRequestsUpdated;
            subscriptionsManager.OutgoingRequestsUpdated += OnOutgoingRequestsUpdated;
        }

        private void ConnectEvents()
        {
            Talo.PlayerPresence.OnPresenceChanged += OnPresenceChanged;
            Talo.PlayerRelationships.OnMessageReceived += OnMessageReceived;
            Talo.PlayerRelationships.OnRelationshipRequestReceived += OnRelationshipRequestReceived;
            Talo.PlayerRelationships.OnRelationshipRequestCancelled += OnRelationshipRequestCancelled;
            Talo.PlayerRelationships.OnRelationshipConfirmed += OnRelationshipConfirmed;
            Talo.PlayerRelationships.OnRelationshipEnded += OnRelationshipEnded;
        }

        private void OnDisable()
        {
            // manager events
            playersManager.PlayersUpdated -= OnPlayersUpdated;
            subscriptionsManager.FriendsUpdated -= OnFriendsUpdated;
            subscriptionsManager.PendingRequestsUpdated -= OnPendingRequestsUpdated;
            subscriptionsManager.OutgoingRequestsUpdated -= OnOutgoingRequestsUpdated;

            Talo.PlayerPresence.OnPresenceChanged -= OnPresenceChanged;
            Talo.PlayerRelationships.OnMessageReceived -= OnMessageReceived;
            Talo.PlayerRelationships.OnRelationshipRequestReceived -= OnRelationshipRequestReceived;
            Talo.PlayerRelationships.OnRelationshipRequestCancelled -= OnRelationshipRequestCancelled;
            Talo.PlayerRelationships.OnRelationshipConfirmed -= OnRelationshipConfirmed;
            Talo.PlayerRelationships.OnRelationshipEnded -= OnRelationshipEnded;
        }

        private async Task IdentifyPlayer()
        {
            playerName = GenerateRandomName();
            await Talo.Players.Identify("username", playerName);
            Talo.CurrentPlayer.SetProp("demo", "friendsDemo");

            playerNameLabel.text = $"You are: {playerName}";
            SendIntroFeedMessages();
        }

        private async Task FindOnlinePlayers()
        {
            // find all players with the "demo" prop set to "friendsDemo"
            var page = await Talo.Players.Search("friendsDemo");

            foreach (var player in page.players)
            {
                var isCurrentPlayer = player.id == Talo.CurrentPlayer.id;
                var isOnline = player.presence != null && player.presence.online;
                if (!isCurrentPlayer && isOnline)
                {
                    var hydratedPresence = await Talo.PlayerPresence.GetPresence(player.id);
                    OnPresenceChanged(hydratedPresence, false, false);
                }
            }
        }

        private void SendIntroFeedMessages()
        {
            uiManager.AddFeedMessage($"Identified as {playerName}", FeedMessageColor.Cyan);
            uiManager.AddFeedMessage("This demo works best with multiple clients running at the same time.", FeedMessageColor.System);
            uiManager.AddFeedMessage("You can use a third-party package like ParrelSync or export a build and run that alongside the editor build.", FeedMessageColor.System);
        }

        private string GenerateRandomName()
        {
            var rng = new System.Random();
            var baseName = RandomNames[rng.Next(RandomNames.Length)];
            var suffix = rng.Next(1, 1000);
            return $"{baseName}{suffix}";
        }

        private async Task RefreshAllData()
        {
            OnPlayersUpdated();
            await subscriptionsManager.RefreshAllData();
        }

        private void OnPlayersUpdated()
        {
            var onlinePlayers = playersManager.GetOnlineAliases();
            uiManager.RefreshPlayersList(onlinePlayers, subscriptionsManager, OnAddFriend);
        }

        private void OnFriendsUpdated()
        {
            uiManager.RefreshFriendsList(subscriptionsManager.GetFriends(), OnRemoveFriend);
            OnPlayersUpdated();
        }

        private void OnPendingRequestsUpdated()
        {
            uiManager.RefreshPendingRequestsList(subscriptionsManager.GetPendingRequests(), OnApproveRequest);
            OnPlayersUpdated();
        }

        private void OnOutgoingRequestsUpdated()
        {
            uiManager.RefreshOutgoingRequestsList(subscriptionsManager.GetOutgoingRequests(), OnCancelRequest);
            OnPlayersUpdated();
        }

        private void OnMessageReceived(PlayerAlias playerAlias, string message)
        {
            uiManager.AddFeedMessage($"[{playerAlias.identifier}]: {message}", FeedMessageColor.Blue);
        }

        private void OnPresenceChanged(PlayerPresence presence, bool onlineChanged, bool customStatusChanged)
        {
            var statusText = presence.online ? "looking for friends" : "offline";
            var statusColor = presence.online ? FeedMessageColor.Green : FeedMessageColor.System;
            var identifier = presence.playerAlias.id == Talo.CurrentAlias.id ? "You are" : (presence.playerAlias.identifier + " is");
            uiManager.AddFeedMessage($"{identifier} now {statusText}", statusColor);
            playersManager.HandlePresenceChanged(presence);
        }

        private async void OnRelationshipRequestReceived(PlayerAlias playerAlias)
        {
            uiManager.AddFeedMessage($"{playerAlias.identifier} sent you a friend request!", FeedMessageColor.Yellow);
            await subscriptionsManager.LoadPendingRequests();
        }

        private async void OnRelationshipRequestCancelled(PlayerAlias playerAlias)
        {
            await subscriptionsManager.LoadPendingRequests();
            await subscriptionsManager.LoadOutgoingRequests();
        }

        private async void OnRelationshipConfirmed(PlayerAlias playerAlias)
        {
            uiManager.AddFeedMessage($"{playerAlias.identifier} accepted your friend request!", FeedMessageColor.Green);
            await subscriptionsManager.LoadFriends();
            await subscriptionsManager.LoadOutgoingRequests();
        }

        private async void OnRelationshipEnded(PlayerAlias playerAlias)
        {
            uiManager.AddFeedMessage($"You are no longer friends with {playerAlias.identifier}", FeedMessageColor.System);
            await subscriptionsManager.LoadFriends();
            await subscriptionsManager.LoadPendingRequests();
        }

        private void OnSendBroadcastPressed()
        {
            var message = broadcastInput.text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            Talo.PlayerRelationships.Broadcast(message);
            uiManager.AddFeedMessage($"[You]: {message}", FeedMessageColor.Green);
            broadcastInput.value = "";
        }

        private async void OnAddFriend(PlayerAlias alias)
        {
            uiManager.AddFeedMessage($"Sending friend request to {alias.identifier}...", FeedMessageColor.Cyan);
            var success = await subscriptionsManager.SendFriendRequest(alias);
            if (success)
            {
                uiManager.AddFeedMessage($"Friend request sent to {alias.identifier}", FeedMessageColor.Yellow);
                await RefreshAllData();
            }
            else
            {
                uiManager.AddFeedMessage($"Failed to send request to {alias.identifier}", FeedMessageColor.Red);
            }
        }

        private async void OnRemoveFriend(PlayerAlias friend)
        {
            uiManager.AddFeedMessage($"Removing {friend.identifier}...", FeedMessageColor.Cyan);
            var success = await subscriptionsManager.RemoveFriend(friend);
            if (success)
            {
                uiManager.AddFeedMessage($"Removed {friend.identifier} from friends", FeedMessageColor.Green);
                await RefreshAllData();
            }
            else
            {
                uiManager.AddFeedMessage($"Failed to remove {friend.identifier}", FeedMessageColor.Red);
            }
        }

        private async void OnApproveRequest(PlayerAlias requester)
        {
            uiManager.AddFeedMessage($"Accepting friend request from {requester.identifier}...", FeedMessageColor.Cyan);
            var success = await subscriptionsManager.AcceptFriendRequest(requester);
            if (success)
            {
                uiManager.AddFeedMessage($"Accepted! {requester.identifier} is now your friend", FeedMessageColor.Green);
                await RefreshAllData();
            }
            else
            {
                uiManager.AddFeedMessage("Failed to accept friend request", FeedMessageColor.Red);
            }
        }

        private async void OnCancelRequest(PlayerAlias player)
        {
            uiManager.AddFeedMessage($"Cancelling friend request to {player.identifier}...", FeedMessageColor.Cyan);
            var success = await subscriptionsManager.RemoveFriend(player);
            if (success)
            {
                uiManager.AddFeedMessage($"Cancelled friend request to {player.identifier}", FeedMessageColor.System);
                await RefreshAllData();
            }
            else
            {
                uiManager.AddFeedMessage($"Failed to cancel request to {player.identifier}", FeedMessageColor.Red);
            }
        }
    }
}
