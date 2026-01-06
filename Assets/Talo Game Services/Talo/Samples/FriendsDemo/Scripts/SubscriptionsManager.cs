using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaloGameServices.Sample.FriendsDemo
{
    public class SubscriptionsManager
    {
        public event Action FriendsUpdated;
        public event Action PendingRequestsUpdated;
        public event Action OutgoingRequestsUpdated;

        private List<PlayerAlias> friends = new();
        private List<PlayerAlias> pendingRequests = new();
        private List<PlayerAlias> outgoingRequests = new();

        public List<PlayerAlias> GetFriends() => friends;
        public List<PlayerAlias> GetPendingRequests() => pendingRequests;
        public List<PlayerAlias> GetOutgoingRequests() => outgoingRequests;

        public async Task RefreshAllData()
        {
            await LoadFriends();
            await LoadOutgoingRequests();
            await LoadPendingRequests();
        }

        public async Task LoadFriends()
        {
            var options = new GetSubscriptionsOptions { confirmed = ConfirmedFilter.Confirmed };
            var page = await Talo.PlayerRelationships.GetSubscriptions(options);
            friends = page.subscriptions.Select((sub) => sub.subscribedTo).ToList();
            FriendsUpdated?.Invoke();
        }

        public async Task LoadPendingRequests()
        {
            var options = new GetSubscribersOptions { confirmed = ConfirmedFilter.Unconfirmed };
            var page = await Talo.PlayerRelationships.GetSubscribers(options);
            pendingRequests = page.subscriptions.Select((sub) => sub.subscriber).ToList();
            PendingRequestsUpdated?.Invoke();
        }

        public async Task LoadOutgoingRequests()
        {
            var options = new GetSubscriptionsOptions { confirmed = ConfirmedFilter.Unconfirmed };
            var page = await Talo.PlayerRelationships.GetSubscriptions(options);
            outgoingRequests = page.subscriptions.Select((sub) => sub.subscribedTo).ToList();
            OutgoingRequestsUpdated?.Invoke();
        }

        public async Task<bool> SendFriendRequest(PlayerAlias alias)
        {
            var subscription = await Talo.PlayerRelationships.SubscribeTo(alias.id, RelationshipType.Bidirectional);
            return subscription != null;
        }

        public async Task<bool> AcceptFriendRequest(PlayerAlias alias)
        {
            return await Talo.PlayerRelationships.ConfirmSubscriptionFrom(alias.id);
        }

        public async Task<bool> RemoveFriend(PlayerAlias alias)
        {
            return await Talo.PlayerRelationships.UnsubscribeFrom(alias.id);
        }

        public bool IsFriend(int aliasId)
        {
            return friends.Any((f) => f.id == aliasId);
        }

        public bool HasPendingRequest(int aliasId)
        {
            return pendingRequests.Any((r) => r.id == aliasId);
        }

        public bool HasOutgoingRequest(int aliasId)
        {
            return outgoingRequests.Any((r) => r.id == aliasId);
        }
    }
}
