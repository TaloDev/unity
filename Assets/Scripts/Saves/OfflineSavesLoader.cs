using UnityEngine;
using TaloGameServices;
using System;

public class OfflineSavesLoader : MonoBehaviour
{
    public void OnButtonClick()
    {
        Save();
    }

    private async void Save()
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
