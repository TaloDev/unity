using System;
using UnityEngine;
using TaloGameServices;
using System.Threading.Tasks;

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
