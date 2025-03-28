using UnityEngine;
using UnityEngine.UIElements;

namespace TaloGameServices.Sample.SavesDemo
{
    public class GlobalUIController : MonoBehaviour
    {
        public UIDocument loginUI, menuUI, savesListUI, gameUI;

        private void Awake()
        {
            SetDocumentVisibility(menuUI, DisplayStyle.None);
            SetDocumentVisibility(savesListUI, DisplayStyle.None);
            SetDocumentVisibility(gameUI, DisplayStyle.None);
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
            if (save != null)
            {
                SetDocumentVisibility(savesListUI, DisplayStyle.None);
                SetDocumentVisibility(menuUI, DisplayStyle.None);
                SetDocumentVisibility(gameUI, DisplayStyle.Flex);
            }
            else
            {
                SetDocumentVisibility(gameUI, DisplayStyle.None);
                SetDocumentVisibility(menuUI, DisplayStyle.Flex);
            }
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

        private void AddNewSaveToList()
        {
            savesListUI.GetComponent<SavesListUIController>().AddSaveToList(Talo.Saves.Latest);
        }
    }
}
