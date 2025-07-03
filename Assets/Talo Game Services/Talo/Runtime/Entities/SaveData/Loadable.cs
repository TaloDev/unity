using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TaloGameServices
{
    public class Loadable : MonoBehaviour, ILoadable
    {
        [SerializeField]
        private string _id = Guid.NewGuid().ToString();

        public string Id => _id;

        private Dictionary<string, object> _savedFields = new();

        public Dictionary<string, object> SavedFields => _savedFields;

        protected virtual void OnEnable()
        {
            Talo.Saves.Register(this);
        }

        public void Hydrate(SavedObjectData[] data)
        {
            var fields = new Dictionary<string, object>();

            foreach (SavedObjectData field in data)
            {
                var type = Type.GetType(field.type);
                fields[field.key] = Convert.ChangeType(field.value, type);
            }

            OnLoaded(fields);
        }

        public virtual void RegisterFields()
        {
            // sometimes all you care about is the loadable's presence in the scene
            // so this can remain unimplemented
        }

        protected void RegisterField(string key, object value)
        {
            _savedFields[key] = value;
        }

        public virtual void OnLoaded(Dictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        protected bool HandleDestroyed(Dictionary<string, object> data)
        {
            data.TryGetValue("meta.destroyed", out var destroyed);
            var isDestroyed = destroyed != null;
            if (isDestroyed) Destroy(gameObject);
            return isDestroyed;
        }

        public string GetPath()
        {
            var go = gameObject;
            name = go.name;
            while (go.transform.parent != null)
            {
                go = go.transform.parent.gameObject;
                name = $"{go.name}.{name}";
            }

            return name;
        }

        public SavedObjectData[] GetLatestData()
        {
            RegisterFields();

            var data = SavedFields.Select((field) => new SavedObjectData()
            {
                key = field.Key,
                value = field.Value.ToString(),
                type = field.Value.GetType().ToString()
            }).ToArray();

            return data;
        }
    }
}
