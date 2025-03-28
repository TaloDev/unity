using UnityEngine;
using UnityEngine.UI;

namespace TaloGameServices.Sample.Playground
{
    public class ToggleOfflineMode : MonoBehaviour
    {
        private void Start()
        {
            SetText();
        }

        private bool GetValue()
        {
            return Talo.Settings.offlineMode;
        }

        private void SetText()
        {
            GetComponentInChildren<Text>().text = GetValue() ? "Go online" : "Go offline";
        }

        public void OnButtonClick()
        {
            Talo.Settings.offlineMode = !Talo.Settings.offlineMode;
            SetText();

            ResponseMessage.SetText($"You are now now {(Talo.Settings.offlineMode ? "offline" : "online")}");
        }
    }
}
