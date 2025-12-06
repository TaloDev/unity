using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TaloGameServices
{
    public class SavesAPI : DebouncedAPI<SavesAPI.DebouncedOperation>
    {
        public enum DebouncedOperation
        {
            Update
        }

        internal SavesManager savesManager;
        internal SavesContentManager contentManager;

        public event Action OnSavesLoaded;
        public event Action OnSaveLoadingCompleted;

        public event Action<GameSave> OnSaveChosen;
        public event Action<GameSave> OnSaveUnloaded;

        public GameSave[] All
        {
            get => savesManager.AllSaves;
        }

        public GameSave Latest
        {
            get => savesManager.GetLatestSave();
        }

        public GameSave Current
        {
            get => savesManager.CurrentSave;
        }

        public SavesAPI() : base("v1/game-saves")
        { }

        internal void Setup()
        {
            savesManager = new();
            contentManager = new();

            savesManager.OnSaveChosen += (save) =>
            {
                OnSaveChosen?.Invoke(save);
            };

            savesManager.OnSavesLoaded += () =>
            {
                OnSavesLoaded?.Invoke();
            };

            contentManager.OnSaveLoadingCompleted += () =>
            {
                OnSaveLoadingCompleted?.Invoke();
            };
        }

        public async Task<GameSave> ReplaceSaveWithOfflineSave(GameSave offlineSave)
        {
            var uri = new Uri($"{baseUrl}/{offlineSave.id}");
            var json = await Call(uri, "PATCH", JsonUtility.ToJson(new SavesPatchRequest
            {
                name = offlineSave.name,
                content = offlineSave.content
            }));

            var res = JsonUtility.FromJson<SavesPostResponse>(json);
            return res.save;
        }

        private async Task<GameSave[]> SyncOfflineSaves(GameSave[] offlineSaves)
        {
            var offlineOnlySaves = offlineSaves.Where((save) => save.id < 0).ToArray();
            if (offlineOnlySaves.Length == 0) return Array.Empty<GameSave>();

            Talo.IdentityCheck();

            var tasks = offlineOnlySaves.Select(async (offlineSave) =>
            {
                try
                {
                    var uri = new Uri(baseUrl);
                    var json = await Call(uri, "POST", JsonUtility.ToJson(new SavesPostRequest
                    {
                        name = offlineSave.name,
                        content = offlineSave.content
                    }));

                    var res = JsonUtility.FromJson<SavesPostResponse>(json);
                    return new { res.save, offlineId = offlineSave.id };
                }
                catch
                {
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);
            var successfulResults = results.Where((res) => res != null);
            var offlineIdsToDelete = successfulResults.Select((res) => res.offlineId).ToArray();
            savesManager.DeleteOfflineSaves(offlineIdsToDelete);

            return successfulResults.Select((res) => res.save).ToArray();
        }

        public async Task<GameSave[]> GetSaves()
        {
            var saves = new List<GameSave>();
            var offlineSaves = savesManager.GetOfflineSavesContent()?.saves;

            if (Talo.IsOffline())
            {
                if (offlineSaves != null) saves.AddRange(offlineSaves);
            }
            else
            {
                Talo.IdentityCheck();

                var json = await Call(GetUri(), "GET");

                var res = JsonUtility.FromJson<SavesIndexResponse>(json);
                var onlineSaves = res.saves;

                if (offlineSaves != null)
                {
                    var tasks = onlineSaves.Select(async (onlineSave) =>
                    {
                        var offlineSave = offlineSaves
                            .FirstOrDefault((offlineSave) => offlineSave.id == onlineSave.id);

                        if (offlineSave != null)
                        {
                            return await savesManager.SyncSave(onlineSave, offlineSave);
                        }
                        return onlineSave;
                    });

                    onlineSaves = await Task.WhenAll(tasks);

                    var syncedSaves = await SyncOfflineSaves(offlineSaves);
                    saves.AddRange(syncedSaves);
                }

                saves.AddRange(onlineSaves);
            }

            savesManager.HandleSavesLoaded(saves);
            return savesManager.AllSaves;
        }

        public void Register(Loadable loadable)
        {
            contentManager.Register(loadable);
        }

        public async Task<GameSave> CreateSave(string saveName, SaveContent content = null)
        {
            GameSave save;
            var saveContent = content ?? contentManager.Content;

            if (Talo.IsOffline())
            {
                save = new GameSave
                {
                    name = saveName,
                    content = saveContent,
                    updatedAt = DateTime.UtcNow.ToString("O")
                };
            }
            else
            {
                Talo.IdentityCheck();

                var uri = new Uri(baseUrl);
                var json = await Call(uri, "POST", JsonUtility.ToJson(new SavesPostRequest
                {
                    name = saveName,
                    content = saveContent
                }));

                var res = JsonUtility.FromJson<SavesPostResponse>(json);
                save = res.save;
            }

            return savesManager.CreateSave(save);
        }

        protected override async Task ExecuteDebouncedOperation(DebouncedOperation operation)
        {
            switch (operation)
            {
                case DebouncedOperation.Update:
                    var currentSave = savesManager.CurrentSave;
                    if (currentSave != null)
                    {
                        await UpdateSave(currentSave.id);
                    }
                    break;
            }
        }

        public void DebounceUpdate()
        {
            Debounce(DebouncedOperation.Update);
        }

        public async Task<GameSave> UpdateCurrentSave(string newName = "")
        {
            var currentSave = savesManager.CurrentSave;
            if (currentSave == null)
            {
                throw new Exception("No save is currently loaded");
            }

            // if the save is being renamed, sync it immediately
            if (!string.IsNullOrEmpty(newName))
            {
                return await UpdateSave(currentSave.id, newName);
            }

            // else, update the save locally and queue it for syncing
            currentSave.content = contentManager.Content;
            DebounceUpdate();
            return currentSave;
        }

        public async Task<GameSave> UpdateSave(int saveId, string newName = "")
        {
            var save = savesManager.FindSaveByID(saveId);

            var saveContent = contentManager.Content;

            if (Talo.IsOffline())
            {
                if (!string.IsNullOrEmpty(newName)) save.name = newName;
                save.content = saveContent;
                save.updatedAt = DateTime.UtcNow.ToString("O");
            }
            else
            {
                Talo.IdentityCheck();

                var uri = new Uri($"{baseUrl}/{saveId}");
                var content = JsonUtility.ToJson(new SavesPatchRequest
                {
                    name = newName,
                    content = saveContent
                });

                var json = await Call(uri, "PATCH", content);

                var res = JsonUtility.FromJson<SavesPostResponse>(json);
                save = res.save;
            }

            savesManager.HandleSaveUpdated(save);

            return save;
        }

        public void ChooseSave(int saveId, bool loadSave = true)
        {
            var save = savesManager.FindSaveByID(saveId);
            savesManager.SetChosenSave(save, loadSave);
        }

        public void UnloadCurrentSave()
        {
            if (Current != null)
            {
                OnSaveUnloaded?.Invoke(Current);
            }

            savesManager.UnloadCurrentSave();
        }

        public async Task DeleteSave(int saveId, bool unloadIfCurrentSave = false)
        {
            var _ = savesManager.FindSaveByID(saveId);

            if (!Talo.IsOffline())
            {
                Talo.IdentityCheck();
                var uri = new Uri($"{baseUrl}/{saveId}");
                await Call(uri, "DELETE");
            }

            savesManager.DeleteSave(saveId, unloadIfCurrentSave);
        }
    }
}
