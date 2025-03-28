using UnityEngine;
using System.Threading.Tasks;

namespace TaloGameServices.Sample.Playground
{
    public class SetProp : MonoBehaviour
    {
        public string key, value;

        public async void OnButtonClick()
        {
            await UpdateProp();
        }

        private async Task UpdateProp()
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                ResponseMessage.SetText("key or value not set on SetPropButton");
                return;
            }

            try
            {
                await Talo.CurrentPlayer.SetProp(key, value);
                ResponseMessage.SetText($"{key} set to {value}");
            }
            catch (System.Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
