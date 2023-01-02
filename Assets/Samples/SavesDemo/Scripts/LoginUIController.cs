using UnityEngine;
using TaloGameServices;
using UnityEngine.UIElements;

public class LoginUIController : MonoBehaviour
{
    private VisualElement root;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("login-btn").clicked += OnLoginClick;
    }

    private async void OnLoginClick()
    {
        var username = root.Q<TextField>().text;
        await Talo.Players.Identify("username", username);
    }
}
