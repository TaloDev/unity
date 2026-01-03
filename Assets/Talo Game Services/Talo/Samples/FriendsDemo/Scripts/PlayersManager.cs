using System.Collections.Generic;
using System.Linq;

namespace TaloGameServices.Sample.FriendsDemo
{
    public class PlayersManager
    {
        public event System.Action PlayersUpdated;

        private readonly Dictionary<int, PlayerAlias> onlineAliases = new();

        public List<PlayerAlias> GetOnlineAliases()
        {
            // exclude the current player
            return onlineAliases.Values
                .Where((alias) => alias.id != Talo.CurrentAlias.id)
                .ToList();
        }

        public void HandlePresenceChanged(PlayerPresence presence)
        {
            var presenceAlias = presence.playerAlias;
            if (presence.online)
            {
                onlineAliases[presenceAlias.id] = presenceAlias;
            }
            else
            {
                onlineAliases.Remove(presenceAlias.id);
            }

            PlayersUpdated?.Invoke();
        }
    }
}
