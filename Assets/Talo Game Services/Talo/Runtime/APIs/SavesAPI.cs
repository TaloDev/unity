using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TaloGameServices
{
    public class SavesAPI : BaseAPI
    {
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
                        })
                            .ToList();

                        onlineSaves = await Task.WhenAll(tasks);

                        var syncedSaves = await savesManager.SyncOfflineSaves(offlineSaves);
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

        public async Task<GameSave> UpdateCurrentSave(string newName = "")
        {
            return await UpdateSave(savesManager.CurrentSave.id, newName);
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

        public async Task DeleteSave(int saveId)
        {
            var save = savesManager.FindSaveByID(saveId);

            if (!Talo.IsOffline())
            {
                Talo.IdentityCheck();
                var uri = new Uri($"{baseUrl}/{saveId}");
                await Call(uri, "DELETE");
            }

            savesManager.DeleteSave(saveId);
        }
    }
}
