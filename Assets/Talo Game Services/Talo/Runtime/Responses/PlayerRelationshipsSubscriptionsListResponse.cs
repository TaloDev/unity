using System;

namespace TaloGameServices
{
    [Serializable]
    public class PlayerRelationshipsSubscriptionsListResponse
    {
        public PlayerAliasSubscription[] subscriptions;
        public int count;
        public int itemsPerPage;
        public bool isLastPage;
    }
}
