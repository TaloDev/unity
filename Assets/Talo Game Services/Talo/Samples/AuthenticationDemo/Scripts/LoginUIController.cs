using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace TaloGameServices.Sample.AuthenticationDemo
{
    public class LoginUIController : MonoBehaviour
    {
        private VisualElement root;

        private void Awake()
        {
            Talo.PlayerAuth.SessionManager.CheckForSession();
        }

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            root.Q<TextField>("username").value = "";
            root.Q<TextField>("password").value = "";
            root.Q<Button>("submit").clicked += OnLoginClick;
            root.Q<Button>("register").clicked += OnGoToRegisterClick;
        }

        private async void OnLoginClick()
        {
            var username = root.Q<TextField>("username").text;
            var password = root.Q<TextField>("password").text;
            var validationLabel = root.Q<Label>("validation-label");

            if (string.IsNullOrEmpty(username))
            {
                validationLabel.text = "Username is required";
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                validationLabel.text = "Password is required";
                return;
            }

            validationLabel.text = "";

            try
            {
                if (await Talo.PlayerAuth.Login(username, password))
                {
                    SendMessageUpwards("GoToVerify", SendMessageOptions.RequireReceiver);
                }
            }
            catch (PlayerAuthException e)
            {
                validationLabel.text = e.ErrorCode switch
                {
                    PlayerAuthErrorCode.INVALID_CREDENTIALS => "Username or password is incorrect",
                    _ => e.Message
                };
            }
            catch (Exception e)
            {
                validationLabel.text = e.Message;
            }
        }

        private void OnGoToRegisterClick()
        {
            SendMessageUpwards("GoToRegister", SendMessageOptions.RequireReceiver);
        }
    }
}
