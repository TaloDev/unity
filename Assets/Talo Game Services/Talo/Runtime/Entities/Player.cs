using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaloGameServices
{
    [Serializable]
    public class Player : EntityWithProps
    {
        public string id;
        public PlayerAlias[] aliases;
        public GroupStub[] groups;
        public PlayerPresence presence;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        public Task<PlayersAPI.PlayerUpdateResult> SetProp(string key, string value, bool update = true)
        {
            base.SetProp(key, value);

            if (update)
            {
                return Talo.Players.DebounceUpdate();
            }
            return Task.FromResult(new PlayersAPI.PlayerUpdateResult(true));
        }

        public Task<PlayersAPI.PlayerUpdateResult> DeleteProp(string key, bool update = true)
        {
            base.DeleteProp(key);

            if (update)
            {
                return Talo.Players.DebounceUpdate();
            }
            return Task.FromResult(new PlayersAPI.PlayerUpdateResult(true));
        }

        public Task<PlayersAPI.PlayerUpdateResult> SetPropArray(string key, IEnumerable<string> values, bool update = true)
        {
            base.SetPropArray(key, values);

            if (update)
            {
                return Talo.Players.DebounceUpdate();
            }
            return Task.FromResult(new PlayersAPI.PlayerUpdateResult(true));
        }

        public Task<PlayersAPI.PlayerUpdateResult> DeletePropArray(string key, bool update = true)
        {
            base.DeletePropArray(key);

            if (update)
            {
                return Talo.Players.DebounceUpdate();
            }
            return Task.FromResult(new PlayersAPI.PlayerUpdateResult(true));
        }

        public Task<PlayersAPI.PlayerUpdateResult> InsertIntoPropArray(string key, string value, bool update = true)
        {
            base.InsertIntoPropArray(key, value);

            if (update)
            {
                return Talo.Players.DebounceUpdate();
            }
            return Task.FromResult(new PlayersAPI.PlayerUpdateResult(true));
        }

        public Task<PlayersAPI.PlayerUpdateResult> RemoveFromPropArray(string key, string value, bool update = true)
        {
            base.RemoveFromPropArray(key, value);

            if (update)
            {
                return Talo.Players.DebounceUpdate();
            }
            return Task.FromResult(new PlayersAPI.PlayerUpdateResult(true));
        }

        public bool IsInGroupID(string groupId)
        {
            return groups.Any((group) => group.id == groupId);
        }

        public bool IsInGroupName(string groupName)
        {
            return groups.Any((group) => group.name == groupName);
        }

        public PlayerAlias GetAlias(string service = "")
        {
            if (string.IsNullOrEmpty(service))
            {
                return aliases.Length > 0 ? aliases[0] : null;
            }

            return aliases.FirstOrDefault((alias) => alias.service == service);
        }
    }
}
