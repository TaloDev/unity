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

        private Dictionary<string, object> _savedFields = new Dictionary<string, object>();

        public string Id => _id;

        public Dictionary<string, object> SavedFields => _savedFields;

        protected virtual void OnEnable()
        {
            Talo.Saves.Register(this);
            Talo.Saves.OnSaveChosen += LoadData;
        }

        protected virtual void OnDisable()
        {
            Talo.Saves.OnSaveChosen -= LoadData;
        }

        private void LoadData(GameSave save)
        {
            if (save == null) return;

            var content = JsonUtility.FromJson<SaveContent>(save.content);
            var fields = new Dictionary<string, object>();
            SavedObject savedObject;

            try
            {
                savedObject = content.objects.First((obj) => obj.id.Equals(Id));
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning($"Loadable with id '{Id}' not found in save '{save.name}'");
                return;
            }

            foreach (SavedObjectData field in savedObject.data)
            {
                var type = Type.GetType(field.type);
                fields.Add(field.key, Convert.ChangeType(field.value, type));
            }

            OnLoaded(fields);
            Talo.Saves.SetObjectLoaded(_id);
        }

        public virtual void RegisterFields()
        {
            throw new NotImplementedException();
        }

        protected void RegisterField(string key, object value)
        {
            _savedFields.Add(key, value);
        }

        public virtual void OnLoaded(Dictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        protected bool HandleDestroyed(Dictionary<string, object> data)
        {
            data.TryGetValue("meta.destroyed", out var destroyed);
            if (destroyed != null) Destroy(gameObject);

            return destroyed != null;
        }
    }
}
