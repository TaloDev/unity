using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TaloGameServices.Sample.SavesDemo
{
    public class GameUIController : MonoBehaviour
    {
        private VisualElement root;
        private Button updateSaveButton;

        public int level;

        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;

            if (Talo.Saves.Current == null)
            {
                GoToMenu();
                return;
            }

            root.Q<Label>("save-name").text = Talo.Saves.Current.name;

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
                GoToMenu();
            };

            SetupLevelButtons();
        }

        private void SetupLevelButtons()
        {
            var buttons = new[] { 1, 2, 3 }.Select((level) =>
            {
                var button = root.Q<Button>($"go-to-level-{level}");
                button.clicked += () =>
                {
                    GoToLevel(level);
                };
                return button;
            }).ToArray();

            buttons[level].SetEnabled(false);
        }

        private void ResetUpdateSaveButtonText()
        {
            updateSaveButton.text = "Save";
        }

        private void GoToMenu()
        {
            GoToScene("SavesDemo");
        }

        private void GoToLevel(int level)
        {
            GoToScene($"Levels/CubesLevel{level}");
        }

        private void GoToScene(string newSceneName)
        {
            var activeScene = SceneManager.GetActiveScene();
            var currentPath = activeScene.path.Split($"Levels/{activeScene.name}.unity")[0];
            var path = $"{currentPath}{newSceneName}.unity";

            EditorSceneManager.LoadSceneAsyncInPlayMode(path, new()
            {
                loadSceneMode = LoadSceneMode.Single
            });
        }
    }
}
