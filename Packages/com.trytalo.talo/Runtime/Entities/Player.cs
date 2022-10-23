using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace TaloGameServices
{
    [Serializable]
    public class Player
    {
        public string id;
        public Prop[] props;
        public Group[] groups;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        public string GetProp(string key, string fallback = null)
        {
            Prop prop = props.First((prop) => prop.key == key);
            return prop?.key ?? fallback;
        }

        public async Task SetProp(string key, string value)
        {
            if (GetProp(key) != null)
            {
                props = props.Select((prop) =>
                {
                    if (prop.key == key) prop.value = value;
                    return prop;
                }).ToArray();
            }
            else
            {
                var propList = props.ToList();
                propList.Add(new Prop((key, value)));
                props = propList.ToArray();
            }

            await Talo.Players.Update();
        }

        public async Task DeleteProp(string key)
        {
            Prop prop = props.First((prop) => prop.key == key);
            if (prop == null) throw new Exception($"Prop with key {key} does not exist");

            prop.value = null;

            await Talo.Players.Update();
        }

        public bool IsInGroup(string groupId)
        {
            return groups.Any((group) => group.id == groupId);
        }
    }
}
