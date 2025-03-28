using UnityEngine;
using UnityEngine.UI;

namespace TaloGameServices.Sample.Playground
{
    public class ToggleContinuity : MonoBehaviour
    {
        private void Start()
        {
            SetText();
        }

        private bool GetValue()
        {
            return Talo.Settings.continuityEnabled;
        }

        private void SetText()
        {
            GetComponentInChildren<Text>().text = GetValue() ? "Toggle off" : "Toggle on";
        }

        public void OnButtonClick()
        {
            Talo.Settings.continuityEnabled = !Talo.Settings.continuityEnabled;
            SetText();

            ResponseMessage.SetText($"Continuity is now {(Talo.Settings.continuityEnabled ? "enabled" : "disabled")}");
        }
    }
}
