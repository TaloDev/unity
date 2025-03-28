using UnityEngine;
using UnityEngine.UIElements;

namespace TaloGameServices.Sample.SavesDemo
{
    public class GameUIController : MonoBehaviour
    {
        private VisualElement root;
        private Button updateSaveButton;

        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;

            updateSaveButton = root.Q<Button>("update-btn");
            updateSaveButton.clicked += async () =>
            {
                await Talo.Saves.UpdateCurrentSave();
                updateSaveButton.text = "Saved!";
                Invoke("ResetUpdateSaveButtonText", 1f);
            };

            root.Q<Button>("back-btn").clicked += () =>
            {
                Talo.Saves.UnloadCurrentSave();
            };
        }

        private void OnEnable()
        {
            Talo.Saves.OnSaveChosen += OnSaveChosen;
        }

        private void OnDisable()
        {
            Talo.Saves.OnSaveChosen -= OnSaveChosen;
        }

        private void OnSaveChosen(GameSave save)
        {
            if (save != null)
            {
                root.Q<Label>("save-name").text = save.name;
            }
        }

        private void ResetUpdateSaveButtonText()
        {
            updateSaveButton.text = "Save";
        }
    }
}
