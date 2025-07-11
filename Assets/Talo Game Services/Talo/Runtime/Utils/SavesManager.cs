using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TaloGameServices
{
    public class SavesManager
    {
        private GameSave _currentSave;
        public GameSave CurrentSave => _currentSave;

        internal List<GameSave> _allSaves = new();
        public GameSave[] AllSaves => _allSaves.ToArray();

        public event Action<GameSave> OnSaveChosen;
        public event Action OnSavesLoaded;

        private IFileHandler<OfflineSavesContent> _fileHandler;

        public SavesManager()
        {
            _fileHandler = Talo.TestMode
                ? new SavesTestFileHandler()
                : new SavesFileHandler();
        }

        public GameSave CreateSave(GameSave save)
        {
            _allSaves.Add(save);

            var offlineSave = UpdateOfflineSaves(save);
            var chosenSave = Talo.IsOffline() ? offlineSave : save;

            SetChosenSave(chosenSave, false);
            return chosenSave;
        }

        public void SetChosenSave(GameSave save, bool loadSave)
        {
            _currentSave = save;
            if (!loadSave) return;

            OnSaveChosen?.Invoke(save);
        }

        public void UnloadCurrentSave()
        {
            SetChosenSave(null, false);
        }

        internal string GetOfflineSavesPath()
        {
            return Application.persistentDataPath + $"/ts.{Talo.CurrentPlayer.id}.bin";
        }

        internal OfflineSavesContent GetOfflineSavesContent()
        {
            return _fileHandler.ReadContent(GetOfflineSavesPath());
        }

        internal void WriteOfflineSavesContent(OfflineSavesContent newContent)
        {
            _fileHandler.WriteContent(GetOfflineSavesPath(), newContent);
        }

        private GameSave CreateOfflineCopy(GameSave originalSave)
        {
            return new GameSave
            {
                id = originalSave.id,
                name = originalSave.name,
                content = originalSave.content,
                updatedAt = originalSave.updatedAt
            };
        }

        public GameSave UpdateOfflineSaves(GameSave incomingSave)
        {
            var offlineIncomingSave = CreateOfflineCopy(incomingSave);
            var offlineContent = GetOfflineSavesContent();
            var updated = false;

            if (offlineContent?.saves != null)
            {
                // updating
                offlineContent.saves = offlineContent.saves.Select((existingSave) =>
                {
                    if (existingSave.id == offlineIncomingSave.id)
                    {
                        updated = true;
                        return offlineIncomingSave;
                    }
                    return existingSave;
                }).ToArray();

                // appending
                if (!updated)
                {
                    if (offlineIncomingSave.id == 0)
                    {
                        offlineIncomingSave.id = -offlineContent.saves.Length - 1;
                    }

                    offlineContent.saves = offlineContent.saves.Concat(new GameSave[] { offlineIncomingSave }).ToArray();
                }
            }
            else
            {
                // first entry into the saves file
                if (offlineIncomingSave.id == 0)
                {
                    offlineIncomingSave.id = -1;
                }
                offlineContent = new OfflineSavesContent(new GameSave[] { offlineIncomingSave });
            }

            WriteOfflineSavesContent(offlineContent);
            return offlineIncomingSave;
        }

        public void DeleteOfflineSave(int saveId)
        {
            var offlineContent = GetOfflineSavesContent();
            offlineContent.saves = offlineContent.saves.Where((save) => save.id != saveId).ToArray();
            WriteOfflineSavesContent(offlineContent);
        }

        public void DeleteSave(int saveId)
        {
            _allSaves = _allSaves.Where((existingSave) => existingSave.id != saveId).ToList();
            DeleteOfflineSave(saveId);

            if (_currentSave?.id == saveId)
            {
                UnloadCurrentSave();
            }
        }

        public async Task<GameSave[]> SyncOfflineSaves(GameSave[] offlineSaves)
        {
            var newSaves = new List<GameSave>();

            foreach (var offlineSave in offlineSaves)
            {
                if (offlineSave.id < 0)
                {
                    var save = await Talo.Saves.CreateSave(offlineSave.name, offlineSave.content);
                    DeleteOfflineSave(offlineSave.id);
                    newSaves.Add(save);
                }
            }

            return newSaves.ToArray();
        }

        public async Task<GameSave> SyncSave(GameSave onlineSave, GameSave offlineSave)
        {
            var onlineUpdatedAt = DateTime.Parse(onlineSave.updatedAt);
            var offlineUpdatedAt = DateTime.Parse(offlineSave.updatedAt);

            if (DateTime.Compare(offlineUpdatedAt, onlineUpdatedAt) > 0)
            {
                var save = await Talo.Saves.ReplaceSaveWithOfflineSave(offlineSave);
                return save;
            }
            else
            {
                return onlineSave;
            }
        }

        public GameSave GetLatestSave()
        {
            if (_allSaves.Count == 0) return null;
            return _allSaves.OrderByDescending((save) => DateTime.Parse(save.updatedAt)).First();
        }

        public void HandleSavesLoaded(List<GameSave> saves)
        {
            _allSaves = saves;
            OnSavesLoaded?.Invoke();

            foreach (var save in _allSaves)
            {
                UpdateOfflineSaves(save);
            }
        }

        public void HandleSaveUpdated(GameSave save)
        {
            _allSaves = _allSaves
                .Select((existingSave) => existingSave.id == save.id ? save : existingSave)
                .ToList();
            UpdateOfflineSaves(save);
        }

        public GameSave FindSaveByID(int saveId)
        {
            GameSave save = _allSaves.FirstOrDefault((existingSave) => existingSave.id == saveId);
            if (save == null) throw new Exception("Save not found");

            return save;
        }
    }
}
