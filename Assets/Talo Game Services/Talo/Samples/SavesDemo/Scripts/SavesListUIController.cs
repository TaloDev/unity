using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace TaloGameServices.Sample.SavesDemo
{
    public class SavesListUIController : MonoBehaviour
    {
        private ListView savesList;
        private Label noSavesLabel;
        public VisualTreeAsset listItemTemplate;
        private List<GameSave> saves;

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            savesList = root.Q<ListView>();
            noSavesLabel = root.Q<Label>("no-saves");

            root.Q<Button>("back-btn").clicked += () =>
            {
                SendMessageUpwards("OnSavesListBackClick", SendMessageOptions.RequireReceiver);
            };
        }

        private void OnEnable()
        {
            Talo.Saves.OnSavesLoaded += OnSavesLoaded;
        }

        private void OnDisable()
        {
            Talo.Saves.OnSavesLoaded -= OnSavesLoaded;
        }

        private void HandleListVisibility()
        {
            if (saves.Count == 0)
            {
                savesList.style.display = DisplayStyle.None;
                noSavesLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                savesList.style.display = DisplayStyle.Flex;
                noSavesLabel.style.display = DisplayStyle.None;
            }
        }

        public void RepopulateList()
        {
            savesList.Rebuild();
            HandleListVisibility();
        }

        public void AddSaveToList(GameSave save)
        {
            saves.Insert(0, save);
            RepopulateList();
        }

        private void OnSavesLoaded()
        {
            saves = new List<GameSave>(Talo.Saves.All);
            saves.Reverse();

            HandleListVisibility();

            savesList.makeItem = () =>
            {
                return listItemTemplate.Instantiate();
            };

            savesList.bindItem = (e, i) =>
            {
                var loadButton = e.Q<Button>("load-btn");
                loadButton.text = saves[i].name;
                loadButton.clicked += () => Talo.Saves.ChooseSave(saves[i].id);

                e.Q<Button>("delete-btn").clicked += async () => {
                    await Talo.Saves.DeleteSave(saves[i].id);
                    savesList.itemsSource.RemoveAt(i);
                    RepopulateList();
                };
            };

            savesList.itemsSource = saves;
        }
    }
}
