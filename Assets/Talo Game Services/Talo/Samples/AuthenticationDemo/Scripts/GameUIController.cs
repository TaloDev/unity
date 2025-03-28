using UnityEngine;
using UnityEngine.UIElements;

namespace TaloGameServices.Sample.AuthenticationDemo
{
    public class GameUIController : MonoBehaviour
    {
        private VisualElement root;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            root.Q<Button>("logout").clicked += OnLogoutClick;
            Talo.Players.OnIdentified += OnIdentified;
        }

        private void OnDisable()
        {
            Talo.Players.OnIdentified -= OnIdentified;
        }

        private void OnIdentified(Player player)
        {
            root.Q<Label>("title").text = $"Hi, {Talo.CurrentAlias.identifier}";
        }

        private async void OnLogoutClick()
        {
            await Talo.PlayerAuth.Logout();
            SendMessageUpwards("GoToLogin", SendMessageOptions.RequireReceiver);
        }
    }
}
