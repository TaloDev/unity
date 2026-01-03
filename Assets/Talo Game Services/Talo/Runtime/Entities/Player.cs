using UnityEngine;
using System.Linq;
using System;

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

        public void SetProp(string key, string value, bool update = true)
        {
            base.SetProp(key, value);

            if (update)
            {
                Talo.Players.DebounceUpdate();
            }
        }

        public void DeleteProp(string key, bool update = true)
        {
            base.DeleteProp(key);

            if (update)
            {
                Talo.Players.DebounceUpdate();
            }
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
