using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public enum ConfirmedFilter
    {
        Any,
        Confirmed,
        Unconfirmed
    }

    public enum RelationshipTypeFilter
    {
        Any,
        Unidirectional,
        Bidirectional
    }

    public class GetSubscribersOptions
    {
        public int page = 0;
        public ConfirmedFilter confirmed = ConfirmedFilter.Any;
        public int aliasId = -1;
        public RelationshipTypeFilter relationshipType = RelationshipTypeFilter.Any;

        public string ToQueryString()
        {
            var query = new Dictionary<string, string> { ["page"] = page.ToString() };

            if (confirmed != ConfirmedFilter.Any)
            {
                // 'true' or 'false'
                query["confirmed"] = (confirmed == ConfirmedFilter.Confirmed).ToString().ToLower();
            }

            if (aliasId != -1)
            {
                query["aliasId"] = aliasId.ToString();
            }

            if (relationshipType != RelationshipTypeFilter.Any)
            {
                var type = relationshipType == RelationshipTypeFilter.Bidirectional ? RelationshipType.Bidirectional : RelationshipType.Unidirectional;
                query["relationshipType"] = PlayerAliasSubscription.RelationshipTypeToString(type);
            }

            return string.Join("&", query.Select((param) => $"{param.Key}={Uri.EscapeDataString(param.Value)}"));
        }
    }

    public class GetSubscriptionsOptions
    {
        public int page = 0;
        public ConfirmedFilter confirmed = ConfirmedFilter.Any;
        public int aliasId = -1;
        public RelationshipTypeFilter relationshipType = RelationshipTypeFilter.Any;

        public string ToQueryString()
        {
            var query = new Dictionary<string, string> { ["page"] = page.ToString() };

            if (confirmed != ConfirmedFilter.Any)
            {
                query["confirmed"] = (confirmed == ConfirmedFilter.Confirmed).ToString().ToLower();
            }

            if (aliasId != -1)
            {
                query["aliasId"] = aliasId.ToString();
            }

            if (relationshipType != RelationshipTypeFilter.Any)
            {
                var type = relationshipType == RelationshipTypeFilter.Bidirectional ? RelationshipType.Bidirectional : RelationshipType.Unidirectional;
                query["relationshipType"] = PlayerAliasSubscription.RelationshipTypeToString(type);
            }

            return string.Join("&", query.Select((param) => $"{param.Key}={Uri.EscapeDataString(param.Value)}"));
        }
    }

    public class PlayerRelationshipsAPI : BaseAPI
    {
        public event Action<PlayerAlias, string> OnMessageReceived;
        public event Action<PlayerAlias> OnRelationshipRequestReceived;
        public event Action<PlayerAlias> OnRelationshipRequestCancelled;
        public event Action<PlayerAlias> OnRelationshipConfirmed;
        public event Action<PlayerAlias> OnRelationshipEnded;

        public PlayerRelationshipsAPI() : base("v1/players/relationships")
        {
            Talo.Socket.OnMessageReceived += HandleSocketMessage;
        }

        private void HandleSocketMessage(SocketResponse response)
        {
            var responseType = response.GetResponseType();

            switch (responseType)
            {
                case "v1.player-relationships.broadcast":
                    var broadcastData = response.GetData<PlayerRelationshipsBroadcastResponse>();
                    if (broadcastData?.playerAlias != null)
                    {
                        OnMessageReceived?.Invoke(broadcastData.playerAlias, broadcastData.message);
                    }
                    break;

                case "v1.player-relationships.subscription-created":
                    var createdData = response.GetData<PlayerRelationshipsSubscriptionCreatedResponse>();
                    if (createdData?.subscription?.subscriber != null)
                    {
                        OnRelationshipRequestReceived?.Invoke(createdData.subscription.subscriber);
                    }
                    break;

                case "v1.player-relationships.subscription-confirmed":
                    var confirmedData = response.GetData<PlayerRelationshipsSubscriptionConfirmedResponse>();
                    if (confirmedData?.subscription != null)
                    {
                        var subscription = confirmedData.subscription;
                        var otherAlias = GetOtherAlias(subscription);
                        if (otherAlias != null)
                        {
                            OnRelationshipConfirmed?.Invoke(otherAlias);
                        }
                    }
                    break;

                case "v1.player-relationships.subscription-deleted":
                    var deletedData = response.GetData<PlayerRelationshipsSubscriptionDeletedResponse>();
                    if (deletedData?.subscription != null)
                    {
                        var subscription = deletedData.subscription;
                        var otherAlias = GetOtherAlias(subscription);
                        if (otherAlias != null)
                        {
                            if (subscription.confirmed)
                            {
                                OnRelationshipEnded?.Invoke(otherAlias);
                            }
                            else
                            {
                                OnRelationshipRequestCancelled?.Invoke(otherAlias);
                            }
                        }
                    }
                    break;
            }
        }

        private PlayerAlias GetOtherAlias(PlayerAliasSubscription subscription)
        {
            if (subscription.subscriber?.player?.id == Talo.CurrentPlayer?.id)
            {
                return subscription.subscribedTo;
            }
            return subscription.subscriber;
        }

        public void Broadcast(string message)
        {
            Talo.IdentityCheck();

            var payload = new PlayerRelationshipsBroadcastRequest
            {
                message = message
            };

            Talo.Socket.Send(new SocketRequest<PlayerRelationshipsBroadcastRequest>("v1.player-relationships.broadcast", payload));
        }

        public async Task<PlayerAliasSubscription> SubscribeTo(int playerAliasId, RelationshipType relationshipType)
        {
            Talo.IdentityCheck();

            var request = new PlayerRelationshipsSubscribeToRequest
            {
                aliasId = playerAliasId,
                relationshipType = PlayerAliasSubscription.RelationshipTypeToString(relationshipType)
            };

            var json = await Call(GetUri(), "POST", JsonUtility.ToJson(request));
            var response = JsonUtility.FromJson<PlayerRelationshipsSubscriptionResponse>(json);
            return response.subscription;
        }

        public async Task RevokeSubscription(int subscriptionId)
        {
            Talo.IdentityCheck();

            await Call(new Uri($"{baseUrl}/{subscriptionId}"), "DELETE");
        }

        public async Task<bool> UnsubscribeFrom(int playerAliasId)
        {
            Talo.IdentityCheck();

            var options = new GetSubscriptionsOptions { aliasId = playerAliasId };
            var page = await GetSubscriptions(options);

            if (page.subscriptions.Length == 0)
            {
                return false;
            }

            await RevokeSubscription(page.subscriptions[0].id);
            return true;
        }

        public async Task<PlayerAliasSubscription> ConfirmSubscriptionById(int subscriptionId)
        {
            Talo.IdentityCheck();

            var json = await Call(new Uri($"{baseUrl}/{subscriptionId}/confirm"), "PUT");
            var response = JsonUtility.FromJson<PlayerRelationshipsSubscriptionResponse>(json);
            return response.subscription;
        }

        public async Task<bool> ConfirmSubscriptionFrom(int playerAliasId)
        {
            Talo.IdentityCheck();

            var options = new GetSubscribersOptions
            {
                confirmed = ConfirmedFilter.Unconfirmed,
                aliasId = playerAliasId
            };
            var page = await GetSubscribers(options);

            if (page.subscriptions.Length == 0)
            {
                return false;
            }

            var subscription = page.subscriptions[0];
            var confirmed = await ConfirmSubscriptionById(subscription.id);
            return confirmed != null;
        }

        public async Task<bool> IsSubscribedTo(int playerAliasId, bool confirmed)
        {
            Talo.IdentityCheck();

            var options = new GetSubscriptionsOptions
            {
                confirmed = confirmed ? ConfirmedFilter.Confirmed : ConfirmedFilter.Any,
                aliasId = playerAliasId
            };
            var page = await GetSubscriptions(options);

            return page.subscriptions.Length > 0;
        }

        public async Task<PlayerRelationshipsSubscriptionsListResponse> GetSubscribers(GetSubscribersOptions options = null)
        {
            Talo.IdentityCheck();

            options ??= new GetSubscribersOptions();

            var queryString = options.ToQueryString();
            var uri = new Uri($"{baseUrl}/subscribers?{queryString}");
            var json = await Call(uri, "GET");
            return JsonUtility.FromJson<PlayerRelationshipsSubscriptionsListResponse>(json);
        }

        public async Task<PlayerRelationshipsSubscriptionsListResponse> GetSubscriptions(GetSubscriptionsOptions options = null)
        {
            Talo.IdentityCheck();

            options ??= new GetSubscriptionsOptions();

            var queryString = options.ToQueryString();
            var uri = new Uri($"{baseUrl}/subscriptions?{queryString}");
            var json = await Call(uri, "GET");
            return JsonUtility.FromJson<PlayerRelationshipsSubscriptionsListResponse>(json);
        }
    }
}
