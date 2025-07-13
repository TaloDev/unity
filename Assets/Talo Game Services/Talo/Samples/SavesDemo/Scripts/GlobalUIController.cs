using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TaloGameServices.Sample.SavesDemo
{
    public class GlobalUIController : MonoBehaviour
    {
        public UIDocument loginUI, menuUI, savesListUI;

        private void Awake()
        {
            SetDocumentVisibility(menuUI, DisplayStyle.None);
            SetDocumentVisibility(savesListUI, DisplayStyle.None);

            if (Talo.CurrentAlias != null)
            {
                OnIdentified(Talo.CurrentPlayer);
            }
        }

        private void OnEnable()
        {
            Talo.Players.OnIdentified += OnIdentified;
            Talo.Saves.OnSaveChosen += OnSaveChosen;
        }

        private void OnDisable()
        {
            Talo.Players.OnIdentified -= OnIdentified;
            Talo.Saves.OnSaveChosen -= OnSaveChosen;
        }

        private async void OnIdentified(Player player)
        {
            await Talo.Saves.GetSaves();
            SetDocumentVisibility(loginUI, DisplayStyle.None);
            SetDocumentVisibility(menuUI, DisplayStyle.Flex);
        }

        private void OnSaveChosen(GameSave save)
        {
            SetDocumentVisibility(savesListUI, DisplayStyle.None);
            SetDocumentVisibility(menuUI, DisplayStyle.None);
            GoToGame();
        }

        private void GoToGame()
        {
            var activeScene = SceneManager.GetActiveScene();
            var currentPath = activeScene.path.Split("SavesDemo.unity")[0];
            var path = currentPath + "Levels/CubesLevel1.unity";

            EditorSceneManager.LoadSceneAsyncInPlayMode(path, new()
            {
                loadSceneMode = LoadSceneMode.Single
            });
        }

        private void SetDocumentVisibility(UIDocument document, DisplayStyle style)
        {
            document.rootVisualElement.style.display = style;
        }

        private void OnSavesListBackClick()
        {
            if (Talo.Saves.All.Length == 0)
            {
                menuUI.GetComponent<MenuUIController>().SetContinueButtonVisibility(DisplayStyle.None);
            }

            SetDocumentVisibility(savesListUI, DisplayStyle.None);
            SetDocumentVisibility(menuUI, DisplayStyle.Flex);
        }

        private void OnLoadSaveClick()
        {
            SetDocumentVisibility(menuUI, DisplayStyle.None);
            SetDocumentVisibility(savesListUI, DisplayStyle.Flex);
        }
    }
}
