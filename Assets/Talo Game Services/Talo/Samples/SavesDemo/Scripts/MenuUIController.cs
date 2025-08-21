using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace TaloGameServices.Sample.SavesDemo
{
    public class MenuUIController : MonoBehaviour
    {
        private Button continueButton;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            continueButton = root.Q<Button>("continue-btn");
            continueButton.clicked += () =>
            {
                Talo.Saves.ChooseSave(Talo.Saves.Latest.id);
            };

            SetContinueButtonVisibility(DisplayStyle.None);

            root.Q<Button>("new-game-btn").clicked += OnNewGameClick;
            root.Q<Button>("load-btn").clicked += OnLoadClick;
        }

        private void OnEnable()
        {
            Talo.Saves.OnSavesLoaded += OnSavesLoaded;
        }

        private void OnDisable()
        {
            Talo.Saves.OnSavesLoaded -= OnSavesLoaded;
        }

        private void OnSavesLoaded()
        {
            if (Talo.Saves.All.Length > 0)
            {
                SetContinueButtonVisibility(DisplayStyle.Flex);
            }
        }

        public void SetContinueButtonVisibility(DisplayStyle style)
        {
            continueButton.style.display = style;
        }

        private async void OnNewGameClick()
        {
            // unload the current save to reset all objects
            Talo.Saves.UnloadCurrentSave();

            var date = DateTime.Now.ToString("ddd dd MMM HH:mm:ss");
            var save = await Talo.Saves.CreateSave($"Save created {date}");
            Talo.Saves.ChooseSave(save.id);
        }

        private void OnLoadClick()
        {
            SendMessageUpwards("OnLoadSaveClick", SendMessageOptions.RequireReceiver);
        }
    }
}
