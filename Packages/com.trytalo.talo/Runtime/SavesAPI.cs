using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TaloGameServices
{
    public class SavesAPI : BaseAPI
    {
        private bool _savesLoaded;
        private GameSave _currentSave;
        private List<GameSave> _allSaves = new List<GameSave>();
        private List<LoadableData> registeredLoadables = new List<LoadableData>();

        public event Action OnSavesLoaded;
        public event Action<GameSave> OnSaveChosen;

        public GameSave[] All
        {
            get => _allSaves.ToArray();
        }

        public GameSave Latest
        {
            get {
                if (_allSaves.Count == 0) return null;
                return _allSaves.OrderByDescending((save) => DateTime.Parse(save.updatedAt)).First();
            }
        }

        public GameSave Current
        {
            get => _currentSave;
        }

        public SavesAPI(TaloSettings settings, HttpClient client) : base(settings, client, "game-saves") {}

        public async Task<GameSave[]> GetSaves()
        {
            Talo.IdentityCheck();

            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(baseUrl + $"?aliasId={Talo.CurrentAlias.id}");

            string json = await Call(req);

            var res = JsonUtility.FromJson<SavesIndexResponse>(json);

            _allSaves = res.saves.ToList();

            if (!_savesLoaded)
            {
                OnSavesLoaded?.Invoke();
                _savesLoaded = true;
            }

            return res.saves;
        }

        public void Register(Loadable loadable)
        {
            registeredLoadables.Add(new LoadableData(loadable));
        }

        public async Task<GameSave> CreateSave(string saveName)
        {
            Talo.IdentityCheck();

            var req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(baseUrl);

            string content = JsonUtility.ToJson(new SavesPostRequest()
            {
                aliasId = Talo.CurrentAlias.id,
                name = saveName,
                content = JsonUtility.ToJson(new SaveContent(registeredLoadables))
            });

            req.Content = new StringContent(content, Encoding.UTF8, "application/json");

            string json = await Call(req);
            var res = JsonUtility.FromJson<SavesPostResponse>(json);

            _allSaves.Add(res.save);
            _currentSave = res.save;

            return _currentSave;
        }

        public async Task<GameSave> UpdateCurrentSave(string newName = "")
        {
            return await UpdateSave(_currentSave.id, string.IsNullOrEmpty(newName) ? _currentSave.name : newName);
        }

        public async Task<GameSave> UpdateSave(int saveId)
        {
            return await UpdateSave(saveId, "");
        }

        public async Task<GameSave> UpdateSave(int saveId, string newName)
        {
            Talo.IdentityCheck();

            GameSave save = _allSaves.First((save) => save.id == saveId);
            if (save == null) throw new Exception("Save not found");

            if (string.IsNullOrEmpty(newName)) newName = save.name;

            var req = new HttpRequestMessage();
            req.Method = new HttpMethod("PATCH");
            req.RequestUri = new Uri(baseUrl + $"/{saveId}");

            string content = JsonUtility.ToJson(new SavesPostRequest()
            {
                aliasId = Talo.CurrentAlias.id,
                name = newName,
                content = JsonUtility.ToJson(new SaveContent(registeredLoadables))
            });

            req.Content = new StringContent(content, Encoding.UTF8, "application/json");

            string json = await Call(req);
            var res = JsonUtility.FromJson<SavesPostResponse>(json);

            _allSaves = _allSaves.Select((save) =>
            {
                if (save.id == saveId) return res.save;
                return save;
            }).ToList();

            _currentSave = res.save;

            return _currentSave;
        }

        public Dictionary<string, object> LoadObject(GameSave save, string id)
        {
            var content = JsonUtility.FromJson<SaveContent>(save.content);

            Dictionary<string, object> fields = new Dictionary<string, object>();
            SavedObject savedObject;

            try
            {
                savedObject = content.objects.First((obj) => obj.id.Equals(id));
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning($"Saved object id '{id}' not found in save '{save.name}'");
                return null;
            }

            foreach (SavedObjectData field in savedObject.data)
            {
                var type = Type.GetType(field.type);
                fields.Add(field.key, Convert.ChangeType(field.value, type));
            }

            return fields;
        }

        public void ChooseSave(GameSave save)
        {
            _currentSave = save;
            OnSaveChosen?.Invoke(save);
        }
    }
}
