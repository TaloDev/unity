using UnityEngine;
using System.Threading.Tasks;
using System;

namespace TaloGameServices.Sample.Playground
{
    public class DeleteHealthProp : MonoBehaviour
    {
        public string key;

        public void OnButtonClick()
        {
            DeleteProp();
        }

        private void DeleteProp()
        {
            if (string.IsNullOrEmpty(key))
            {
                ResponseMessage.SetText("key not set on DeletePropButton");
                return;
            }

            try
            {
                Talo.CurrentPlayer.DeleteProp(key);
                ResponseMessage.SetText($"{key} deleted");
            }
            catch (Exception ex)
            {
                ResponseMessage.SetText(ex.Message);
                throw;
            }
        }
    }
}
