using UnityEngine;
using System.Threading.Tasks;

namespace TaloGameServices.Sample.Playground
{
    public class SetProp : MonoBehaviour
    {
        public string key, value;

        public void OnButtonClick()
        {
            UpdateProp();
        }

        private void UpdateProp()
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                ResponseMessage.SetText("key or value not set on SetPropButton");
                return;
            }

            try
            {
                Talo.CurrentPlayer.SetProp(key, value);
                ResponseMessage.SetText($"{key} set to {value}");
            }
            catch (System.Exception ex)
            {
                ResponseMessage.SetText(ex.Message);
                throw;
            }
        }
    }
}
