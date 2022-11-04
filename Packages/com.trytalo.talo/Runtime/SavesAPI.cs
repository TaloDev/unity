using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TaloGameServices
{
    public class SavesAPI : BaseAPI
    {
        private GameSave _currentSave;
        internal List<GameSave> _allSaves = new();
        internal List<LoadableData> _registeredLoadables = new();
        private List<string> _loadedLoadables = new();

        public event Action OnSavesLoaded;
        public event Action<GameSave> OnSaveChosen;
        public event Action OnSaveLoadingCompleted;

        private readonly string _offlineSavesPath = Application.persistentDataPath + "/saves.json";
        private IFileHandler<OfflineSavesContent> _fileHandler;

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

        public SavesAPI(TaloManager manager) : base(manager, "game-saves") {
            _fileHandler = Talo.TestMode
                ? new SavesTestFileHandler()
                : new SavesFileHandler();
        }

        private async Task<GameSave> ReplaceSaveWithOfflineSave(GameSave offlineSave)
        {
            var uri = new Uri(baseUrl + $"/{offlineSave.id}");

            var content = JsonUtility.ToJson(new SavesPatchRequest()
            {
                name = offlineSave.name,
                content = offlineSave.content
            });

            var json = await Call(uri, "PATCH", content);
            var res = JsonUtility.FromJson<SavesPostResponse>(json);

            DeleteOfflineSave(offlineSave.id);

            return res.save;
        }

        private async Task<GameSave> SyncSave(GameSave onlineSave, GameSave offlineSave)
        {
            var onlineUpdatedAt = DateTime.Parse(onlineSave.updatedAt).ToUniversalTime();
            var offlineUpdatedAt = DateTime.Parse(offlineSave.updatedAt).ToUniversalTime();

            if (offlineUpdatedAt > onlineUpdatedAt)
            {
                return await ReplaceSaveWithOfflineSave(offlineSave);
            }
            else
            {
                return onlineSave;
            }
        }

        private async Task<GameSave[]> SyncOfflineSaves(GameSave[] offlineSaves)
        {
            var newSaves = new List<GameSave>();

            foreach (var offlineSave in offlineSaves)
            {
                if (offlineSave.id < 0)
                {
                    var save = await CreateSave(offlineSave.name, offlineSave.content);
                    DeleteOfflineSave(offlineSave.id);
                    newSaves.Add(save);
                }
            }

            return newSaves.ToArray();
        }

        public async Task<GameSave[]> GetSaves()
        {
            var saves = new List<GameSave>();
            var offlineSaves = GetOfflineSavesContent()?.saves;

            if (Talo.IsOffline())
            {
                if (offlineSaves != null) saves.AddRange(offlineSaves);
            } else
            {
                Talo.IdentityCheck();

                var json = await Call(GetUri(), "GET");
                var res = JsonUtility.FromJson<SavesIndexResponse>(json);
                var onlineSaves = res.saves;

                if (offlineSaves != null)
                {
                    onlineSaves = onlineSaves.Select(async (onlineSave) =>
                    {
                        var offlineSave = offlineSaves
                            .Where((offlineSave) => offlineSave.id == onlineSave.id)
                            .FirstOrDefault();

                        if (offlineSave != null)
                        {
                            return await SyncSave(onlineSave, offlineSave);
                        }
                        return onlineSave;
                    })
                        .Select((t) => t.Result)
                        .ToArray();

                    var syncedSaves = await SyncOfflineSaves(offlineSaves);
                    saves.AddRange(syncedSaves);
                }

                saves.AddRange(onlineSaves);
            }

            _allSaves = saves;
            OnSavesLoaded?.Invoke();

            foreach(var save in _allSaves)
            {
                UpdateOfflineSaves(save);
            }

            return _allSaves.ToArray();
        }

        public void Register(Loadable loadable)
        {
            _registeredLoadables.Add(new LoadableData(loadable));
        }

        internal OfflineSavesContent GetOfflineSavesContent()
        {
            return _fileHandler.ReadContent(_offlineSavesPath);
        }

        internal void WriteOfflineSavesContent(OfflineSavesContent newContent)
        {
            _fileHandler.WriteContent(_offlineSavesPath, newContent);
        }

        private void UpdateOfflineSaves(GameSave incomingSave)
        {
            var offlineContent = GetOfflineSavesContent();
            var updated = false;

            if (offlineContent?.saves != null)
            {
                // updating
                offlineContent.saves = offlineContent.saves.Select((existingSave) =>
                {
                    if (existingSave.id == incomingSave.id)
                    {
                        updated = true;
                        return incomingSave;
                    }
                    return existingSave;
                }).ToArray();

                // appending
                if (!updated)
                {
                    if (incomingSave.id == 0)
                    {
                        incomingSave.id = -offlineContent.saves.Length - 1;
                    }

                    offlineContent.saves = offlineContent.saves.Concat(new GameSave[] { incomingSave }).ToArray();
                }
            } else
            {
                // first entry into the saves file
                incomingSave.id = -1;
                offlineContent = new OfflineSavesContent(new GameSave[] { incomingSave });
            }

            WriteOfflineSavesContent(offlineContent);
        }

        public async Task<GameSave> CreateSave(string saveName, string content = null)
        {
            GameSave save;

            string saveContent = content ?? JsonUtility.ToJson(new SaveContent(_registeredLoadables));

            if (Talo.IsOffline())
            {
                save = new GameSave
                {
                    name = saveName,
                    content = saveContent,
                    updatedAt = DateTime.UtcNow.ToString("O")
                };
            } else
            {
                Talo.IdentityCheck();

                var uri = new Uri(baseUrl);
                var json = await Call(uri, "POST", JsonUtility.ToJson(new SavesPostRequest()
                {
                    name = saveName,
                    content = saveContent
                }));

                var res = JsonUtility.FromJson<SavesPostResponse>(json);
                save = res.save;
            }

            _allSaves.Add(save);
            UpdateOfflineSaves(save);

            ChooseSave(save, true);
            return save;
        }

        public async Task<GameSave> UpdateCurrentSave(string newName = "")
        {
            return await UpdateSave(_currentSave.id, newName);
        }

        public async Task<GameSave> UpdateSave(int saveId, string newName = "")
        {
            GameSave save = _allSaves.First((existingSave) => existingSave.id == saveId);
            if (save == null) throw new Exception("Save not found");

            var saveContent = JsonUtility.ToJson(new SaveContent(_registeredLoadables));

            if (Talo.IsOffline())
            {
                if (!string.IsNullOrEmpty(newName)) save.name = newName;
                save.content = saveContent;
                save.updatedAt = DateTime.UtcNow.ToString("O");
            } else
            {
                Talo.IdentityCheck();

                var uri = new Uri(baseUrl + $"/{saveId}");
                var content = JsonUtility.ToJson(new SavesPatchRequest()
                {
                    name = newName,
                    content = saveContent
                });

                var json = await Call(uri, "PATCH", content);
                var res = JsonUtility.FromJson<SavesPostResponse>(json);
                save = res.save;
            }

            _allSaves = _allSaves
                .Select((existingSave) => existingSave.id == saveId ? save : existingSave)
                .ToList();
            UpdateOfflineSaves(save);

            return save;
        }

        public void ChooseSave(GameSave save, bool newSave = false)
        {
            _currentSave = save;
            if (newSave) return;

            _loadedLoadables.Clear();
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

        private void DeleteOfflineSave(int saveId)
        {
            var offlineContent = GetOfflineSavesContent();
            offlineContent.saves = offlineContent.saves.Where((save) => save.id != saveId).ToArray();
            WriteOfflineSavesContent(offlineContent);
        }

        public async Task DeleteSave(int saveId)
        {
            GameSave save = _allSaves.First((existingSave) => existingSave.id == saveId);
            if (save == null) throw new Exception("Save not found");

            if (!Talo.IsOffline())
            {
                Talo.IdentityCheck();
                var uri = new Uri(baseUrl + $"/{saveId}");
                await Call(uri, "DELETE");
            }

            _allSaves = _allSaves.Where((existingSave) => existingSave.id != saveId).ToList();
            DeleteOfflineSave(saveId);

            if (_currentSave.id == saveId)
            {
                ChooseSave(null);
            }
        }
    }
}
