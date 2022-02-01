﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TaloGameServices
{
    public enum SaveMode
    {
        OFFLINE_ONLY,
        ONLINE_ONLY,
        BOTH
    }

    public class SavesAPI : BaseAPI
    {
        private GameSave _currentSave;
        private List<GameSave> _allSaves = new List<GameSave>();
        private List<LoadableData> _registeredLoadables = new List<LoadableData>();
        private List<string> _loadedLoadables = new List<string>();

        public event Action OnSavesLoaded;
        public event Action<GameSave> OnSaveChosen;
        public event Action OnSaveLoadingCompleted;

        private readonly string _offlineSavesPath = Application.persistentDataPath + "/saves.json";

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

        public async Task<GameSave[]> GetSaves(SaveMode mode = SaveMode.BOTH)
        {
            if (mode != SaveMode.OFFLINE_ONLY) Talo.IdentityCheck();

            var saves = new List<GameSave>();

            if (mode != SaveMode.ONLINE_ONLY && File.Exists(_offlineSavesPath))
            {
                saves.AddRange(GetOfflineSavesContent().saves);
            }

            if (mode != SaveMode.OFFLINE_ONLY)
            {
                try
                {
                    var req = new HttpRequestMessage();
                    req.Method = HttpMethod.Get;
                    req.RequestUri = new Uri(baseUrl + $"?aliasId={Talo.CurrentAlias.id}");

                    string json = await Call(req);
                    var res = JsonUtility.FromJson<SavesIndexResponse>(json);
                    saves.AddRange(res.saves);
                } catch
                {
                    Debug.LogWarning("Failed to load online saves");
                }
            }

            _allSaves = saves;

            OnSavesLoaded?.Invoke();

            return _allSaves.ToArray();
        }

        public void Register(Loadable loadable)
        {
            _registeredLoadables.Add(new LoadableData(loadable));
        }

        private OfflineSavesContent GetOfflineSavesContent()
        {
            var sr = new StreamReader(_offlineSavesPath);
            var content = sr.ReadToEnd();
            sr.Close();

            return JsonUtility.FromJson<OfflineSavesContent>(content);
        }

        private void UpdateOfflineSaves(GameSave incomingSave)
        {
            var offlineContent = GetOfflineSavesContent();

            var updated = false;
            offlineContent.saves = offlineContent.saves.Select((existingSave) =>
            {
                if (existingSave.id == incomingSave.id)
                {
                    updated = true;
                    return incomingSave;
                }
                return existingSave;
            }).ToArray();

            if (!updated)
            {
                offlineContent.saves = offlineContent.saves.Concat(new GameSave[] { incomingSave }).ToArray();
            }

            var sw = new StreamWriter(_offlineSavesPath);
            sw.WriteLine(JsonUtility.ToJson(offlineContent));
            sw.Close();
        }

        public async Task<GameSave> CreateSave(string saveName, SaveMode mode = SaveMode.BOTH)
        {
            if (mode != SaveMode.OFFLINE_ONLY) Talo.IdentityCheck();

            GameSave save = null;
            string saveContent = JsonUtility.ToJson(new SaveContent(_registeredLoadables));

            if (mode != SaveMode.ONLINE_ONLY)
            {
                save = new GameSave();
                save.name = saveName;
                save.content = saveContent;
                save.updatedAt = DateTime.UtcNow.ToString("O");
                save.id = -save.GetHashCode();
                UpdateOfflineSaves(save);
            }

            if (mode != SaveMode.OFFLINE_ONLY)
            {
                try
                {
                    var req = new HttpRequestMessage();
                    req.Method = HttpMethod.Post;
                    req.RequestUri = new Uri(baseUrl);

                    string content = JsonUtility.ToJson(new SavesPostRequest()
                    {
                        aliasId = Talo.CurrentAlias.id,
                        name = saveName,
                        content = saveContent
                    });

                    req.Content = new StringContent(content, Encoding.UTF8, "application/json");

                    string json = await Call(req);
                    var res = JsonUtility.FromJson<SavesPostResponse>(json);
                    save = res.save;
                } catch
                {
                    Debug.LogWarning("Failed to create online save");
                }
            }

            _allSaves.Add(save);
            _currentSave = save;

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
            GameSave save = _allSaves.First((save) => save.id == saveId);
            if (save == null) throw new Exception("Save not found");

            if (save.id > 0) Talo.IdentityCheck();

            if (string.IsNullOrEmpty(newName)) newName = save.name;

            var saveContent = JsonUtility.ToJson(new SaveContent(_registeredLoadables));

            if (save.id < 0)
            {
                save.name = newName;
                save.content = saveContent;
                save.updatedAt = DateTime.UtcNow.ToString("O");
                UpdateOfflineSaves(save);
            }
            else
            {
                var req = new HttpRequestMessage();
                req.Method = new HttpMethod("PATCH");
                req.RequestUri = new Uri(baseUrl + $"/{saveId}");

                string content = JsonUtility.ToJson(new SavesPostRequest()
                {
                    aliasId = Talo.CurrentAlias.id,
                    name = newName,
                    content = saveContent
                });

                req.Content = new StringContent(content, Encoding.UTF8, "application/json");

                string json = await Call(req);
                var res = JsonUtility.FromJson<SavesPostResponse>(json);
                save = res.save;
            }

            _allSaves = _allSaves.Select((save) =>
            {
                if (save.id == saveId) return save;
                return save;
            }).ToList();

            _currentSave = save;

            return _currentSave;
        }

        public Dictionary<string, object> LoadObject(GameSave save, Loadable loadable)
        {
            var content = JsonUtility.FromJson<SaveContent>(save.content);

            Dictionary<string, object> fields = new Dictionary<string, object>();
            SavedObject savedObject;

            try
            {
                savedObject = content.objects.First((obj) => obj.id.Equals(loadable.Id));
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning($"Loadable with id '{loadable.Id}' not found in save '{save.name}'", loadable);
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
            _loadedLoadables.Clear();
            _currentSave = save;
            OnSaveChosen?.Invoke(save);
        }

        public void SetObjectLoaded(string id)
        {
            _loadedLoadables.Add(id);
            if (_loadedLoadables.Count == _registeredLoadables.Count)
            {
                OnSaveLoadingCompleted?.Invoke();
            }
        }
    }
}
