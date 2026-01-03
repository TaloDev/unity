using UnityEngine.UIElements;
using System.Collections.Generic;

namespace TaloGameServices.Sample.FriendsDemo
{
    public enum FeedMessageColor
    {
        Default,
        System,
        Cyan,
        Green,
        Yellow,
        Red,
        Blue
    }

    public class UIManager
    {
        private readonly ScrollView feedScrollView;
        private readonly ScrollView playersScrollView;
        private readonly ScrollView friendsScrollView;
        private readonly ScrollView pendingRequestsScrollView;
        private readonly ScrollView outgoingRequestsScrollView;

        public UIManager(
            ScrollView feedScrollView,
            ScrollView playersScrollView,
            ScrollView friendsScrollView,
            ScrollView pendingRequestsScrollView,
            ScrollView outgoingRequestsScrollView)
        {
            this.feedScrollView = feedScrollView;
            this.playersScrollView = playersScrollView;
            this.friendsScrollView = friendsScrollView;
            this.pendingRequestsScrollView = pendingRequestsScrollView;
            this.outgoingRequestsScrollView = outgoingRequestsScrollView;
        }

        public void AddFeedMessage(string message, FeedMessageColor color = FeedMessageColor.Default)
        {
            var label = new Label(message);
            label.AddToClassList("feed-message");

            if (color != FeedMessageColor.Default)
            {
                var colorClass = color.ToString().ToLower();
                label.AddToClassList($"feed-message-{colorClass}");
            }

            feedScrollView.Add(label);
            feedScrollView.schedule.Execute(() => feedScrollView.ScrollTo(label));
        }

        public void RefreshPlayersList(
            List<PlayerAlias> onlinePlayers,
            SubscriptionsManager subscriptionsManager,
            System.Action<PlayerAlias> onAddFriend)
        {
            ClearContainer(playersScrollView);

            foreach (var player in onlinePlayers)
            {
                var container = new VisualElement();
                container.AddToClassList("player-item");

                var nameLabel = new Label(player.identifier);
                nameLabel.AddToClassList("player-name");
                container.Add(nameLabel);

                var isFriend = subscriptionsManager.IsFriend(player.id);
                var hasPendingRequest = subscriptionsManager.HasPendingRequest(player.id);
                var hasOutgoingRequest = subscriptionsManager.HasOutgoingRequest(player.id);

                if (isFriend)
                {
                    var statusLabel = new Label("Friend");
                    statusLabel.AddToClassList("player-status");
                    container.Add(statusLabel);
                }
                else if (hasPendingRequest)
                {
                    var statusLabel = new Label("Pending");
                    statusLabel.AddToClassList("player-status");
                    container.Add(statusLabel);
                }
                else if (hasOutgoingRequest)
                {
                    var statusLabel = new Label("Sent");
                    statusLabel.AddToClassList("player-status");
                    container.Add(statusLabel);
                }
                else
                {
                    var addBtn = new Button(() => onAddFriend(player)) { text = "Add Friend" };
                    addBtn.AddToClassList("small-button");
                    container.Add(addBtn);
                }

                playersScrollView.Add(container);
            }
        }

        public void RefreshFriendsList(List<PlayerAlias> friends, System.Action<PlayerAlias> onRemoveFriend)
        {
            ClearContainer(friendsScrollView);

            foreach (var friend in friends)
            {
                var container = new VisualElement();
                container.AddToClassList("player-item");

                var nameLabel = new Label(friend.identifier);
                nameLabel.AddToClassList("player-name");
                container.Add(nameLabel);

                var removeBtn = new Button(() => onRemoveFriend(friend)) { text = "Remove" };
                removeBtn.AddToClassList("small-button");
                container.Add(removeBtn);

                friendsScrollView.Add(container);
            }
        }

        public void RefreshPendingRequestsList(List<PlayerAlias> requests, System.Action<PlayerAlias> onApproveRequest)
        {
            ClearContainer(pendingRequestsScrollView);

            foreach (var request in requests)
            {
                var container = new VisualElement();
                container.AddToClassList("player-item");

                var nameLabel = new Label(request.identifier);
                nameLabel.AddToClassList("player-name");
                container.Add(nameLabel);

                var acceptBtn = new Button(() => onApproveRequest(request)) { text = "Accept" };
                acceptBtn.AddToClassList("small-button");
                container.Add(acceptBtn);

                pendingRequestsScrollView.Add(container);
            }
        }

        public void RefreshOutgoingRequestsList(List<PlayerAlias> requests, System.Action<PlayerAlias> onCancelRequest)
        {
            ClearContainer(outgoingRequestsScrollView);

            foreach (var request in requests)
            {
                var container = new VisualElement();
                container.AddToClassList("player-item");

                var nameLabel = new Label(request.identifier);
                nameLabel.AddToClassList("player-name");
                container.Add(nameLabel);

                var cancelBtn = new Button(() => onCancelRequest(request)) { text = "Cancel" };
                cancelBtn.AddToClassList("small-button");
                container.Add(cancelBtn);

                outgoingRequestsScrollView.Add(container);
            }
        }

        private void ClearContainer(ScrollView scrollView)
        {
            scrollView.Clear();
        }
    }
}
