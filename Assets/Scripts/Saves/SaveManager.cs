using UnityEngine;
using TaloGameServices;
using System;

public class SaveManager : MonoBehaviour
{
    public string saveName = "New Save";
    public bool updateCurrentSave;

    public void OnButtonClick()
    {
        Save();
    }

    private async void Save()
    {
        try
        {
            GameSave save;

            if (updateCurrentSave)
            {
                save = await Talo.Saves.UpdateCurrentSave(saveName);
            } else
            {
                save = await Talo.Saves.CreateSave(saveName);
            }

            ResponseMessage.SetText($"Save '{save.name}' {(updateCurrentSave ? "updated" : "created")}");
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
