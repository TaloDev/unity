using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace TaloGameServices.Sample.AuthenticationDemo
{
    public class VerifyUIController : MonoBehaviour
    {
        private VisualElement root;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            root.Q<TextField>("code").value = "";
            root.Q<Button>("submit").clicked += OnSubmitClicked;
        }

        private async void OnSubmitClicked()
        {
            var code = root.Q<TextField>("code").text;
            var validationLabel = root.Q<Label>("validation-label");

            if (string.IsNullOrEmpty(code))
            {
                validationLabel.text = "Verification code is required";
                return;
            }

            validationLabel.text = "";

            try
            {
                await Talo.PlayerAuth.Verify(code);
            }
            catch (PlayerAuthException e)
            {
                validationLabel.text = e.ErrorCode switch
                {
                    PlayerAuthErrorCode.VERIFICATION_CODE_INVALID => "Verification code is incorrect",
                    _ => e.Message
                };
            }
            catch (Exception e)
            {
                validationLabel.text = e.Message;
            }
        }
    }
}
