using UnityEngine;
using System.Linq;
using System;

namespace TaloGameServices {
    [Serializable]
    public class Player {
        public string id;
        public Prop[] props;

        public override string ToString() {
            return JsonUtility.ToJson(this);
        }

        public string GetProp(string key, string fallback) {
            Prop prop = props.First((prop) => prop.key == key);
            return prop?.key ?? fallback;
        }

        public void SetProp(string key, string value) {
            props = props.Select((prop) => {
                if (prop.key == key) prop.value = value;
                return prop;
            }).ToArray();

            Talo.Players.Update();
        }

        public void DeleteProp(string key) {
            Prop prop = props.First((prop) => prop.key == key);
            if (prop == null) throw new Exception($"Prop with key {key} does not exist");

            prop.value = null;

            Talo.Players.Update();
        }
    }
}
