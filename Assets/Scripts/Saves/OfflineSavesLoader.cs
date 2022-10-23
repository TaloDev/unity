using UnityEngine;
using TaloGameServices;
using System;
using System.Threading.Tasks;

public class OfflineSavesLoader : MonoBehaviour
{
    public async void OnButtonClick()
    {
        await Save();
    }

    private async Task Save()
    {
        try
        {
            await Talo.Saves.GetSaves(SaveMode.OFFLINE_ONLY);
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
