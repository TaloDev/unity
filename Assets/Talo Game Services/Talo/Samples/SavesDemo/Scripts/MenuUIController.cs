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
            foreach (var cube in FindObjectsByType<LoadableCube>(FindObjectsSortMode.None))
            {
                cube.MoveToOriginalPos();
            }

            var date = DateTime.Now.ToString("ddd dd MMM HH:mm:ss");
            var save = await Talo.Saves.CreateSave($"Save created {date}");
            Talo.Saves.ChooseSave(save.id);

            SendMessageUpwards("AddNewSaveToList", SendMessageOptions.RequireReceiver);

            SetContinueButtonVisibility(DisplayStyle.Flex);
        }

        private void OnLoadClick()
        {
            SendMessageUpwards("OnLoadSaveClick", SendMessageOptions.RequireReceiver);
        }
    }
}
