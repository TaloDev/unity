using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace TaloGameServices
{
    [Serializable]
    public class Player: EntityWithProps
    {
        public string id;
        public Group[] groups;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        public async Task SetProp(string key, string value, bool update = true)
        {
            base.SetProp(key, value);

            if (update)
            {
                await Talo.Players.Update();
            }
        }

        public async Task DeleteProp(string key, bool update = true)
        {
            base.DeleteProp(key);

            if (update)
            {
                await Talo.Players.Update();
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
    }
}
