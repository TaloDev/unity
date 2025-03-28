using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace TaloGameServices.Sample.AuthenticationDemo
{
    public class RegisterUIController : MonoBehaviour
    {
        private VisualElement root;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            root.Q<TextField>("username").value = "";
            root.Q<TextField>("password").value = "";
            root.Q<TextField>("email").value = "";
            root.Q<Button>("submit").clicked += OnRegisterClick;
            root.Q<Button>("login").clicked += OnGoToLoginClick;
        }

        private async void OnRegisterClick()
        {
            var username = root.Q<TextField>("username").text;
            var password = root.Q<TextField>("password").text;
            var enableVerification = root.Q<Toggle>("enable-verification").value;
            var email = root.Q<TextField>("email").text;

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
                await Talo.PlayerAuth.Register(username, password, email, enableVerification);
            }
            catch (PlayerAuthException e)
            {
                validationLabel.text = e.ErrorCode switch
                {
                    PlayerAuthErrorCode.IDENTIFIER_TAKEN => "Username is already taken",
                    PlayerAuthErrorCode.INVALID_EMAIL => "Invalid email address",
                    _ => e.Message
                };
            }
            catch (Exception e)
            {
                validationLabel.text = e.Message;
            }
        }

        private void OnGoToLoginClick()
        {
            SendMessageUpwards("GoToLogin", SendMessageOptions.RequireReceiver);
        }
    }
}
