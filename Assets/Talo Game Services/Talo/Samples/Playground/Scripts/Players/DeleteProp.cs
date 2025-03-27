using UnityEngine;
using TaloGameServices;
using System.Threading.Tasks;
using System;

public class DeleteHealthProp : MonoBehaviour
{
    public string key;

    public async void OnButtonClick()
    {
        await DeleteProp();
    }

    private async Task DeleteProp()
    {
        if (string.IsNullOrEmpty(key))
        {
            ResponseMessage.SetText("key not set on DeletePropButton");
            return;
        }

        try
        {
            await Talo.CurrentPlayer.DeleteProp(key);
            ResponseMessage.SetText($"{key} deleted");
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
