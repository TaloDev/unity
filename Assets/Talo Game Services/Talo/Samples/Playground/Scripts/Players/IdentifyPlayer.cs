using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace TaloGameServices.Sample.Playground
{
    public class IdentifyPlayer : MonoBehaviour
    {
        public string service, identifier;

        private void Start()
        {
            Talo.Players.OnIdentified += OnIdentified;
        }

        public async void OnButtonClick()
        {
            await Identify();
        }

        private async Task Identify()
        {
            if (string.IsNullOrEmpty(service) || string.IsNullOrEmpty(identifier))
            {
                ResponseMessage.SetText("service or identifier not set on IdentifyButton");
                return;
            }

            try
            {
                await Talo.Players.Identify(service, identifier);
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }

        private void OnIdentified(Player player)
        {
            var panel = GameObject.Find("APIs");
            if (panel != null)
            {
                ResponseMessage.SetText("Identified!");
                panel.GetComponent<Image>().color = new Color(120 / 255f, 230 / 255f, 160 / 255f);
            }
        }
    }
}
