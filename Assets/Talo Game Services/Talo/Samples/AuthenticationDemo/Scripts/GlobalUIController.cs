using UnityEngine;
using UnityEngine.UIElements;

namespace TaloGameServices.Sample.AuthenticationDemo
{
    public class GlobalUIController : MonoBehaviour
    {
        public UIDocument loginUI, registerUI, verifyUI, gameUI;

        private void Awake()
        {
            ShowDocument(loginUI);
        }

        private void OnEnable()
        {
            Talo.Players.OnIdentified += OnIdentified;
        }

        private void OnDisable()
        {
            Talo.Players.OnIdentified -= OnIdentified;
        }

        private void OnIdentified(Player player)
        {
            GoToGame();
        }

        private void ShowDocument(UIDocument document)
        {
            foreach (var d in new [] { loginUI, registerUI, verifyUI, gameUI })
            {
                d.rootVisualElement.style.display = d == document ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void GoToRegister()
        {
            ShowDocument(registerUI);
        }

        private void GoToLogin()
        {
            ShowDocument(loginUI);
        }

        private void GoToVerify()
        {
            ShowDocument(verifyUI);
        }

        private void GoToGame()
        {
            ShowDocument(gameUI);
        }
    }
}
