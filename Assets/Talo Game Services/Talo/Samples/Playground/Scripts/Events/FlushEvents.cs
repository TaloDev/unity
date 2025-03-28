using System;
using UnityEngine;
using System.Threading.Tasks;

namespace TaloGameServices.Sample.Playground
{
    public class FlushEvents : MonoBehaviour
    {
        public async void OnButtonClick()
        {
            await Flush();
        }

        private async Task Flush()
        {
            try
            {
                await Talo.Events.Flush();

                ResponseMessage.SetText("Flushed events");
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
