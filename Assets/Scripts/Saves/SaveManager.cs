using UnityEngine;
using TaloGameServices;
using System;

public class SaveManager : MonoBehaviour
{
    public string saveName = "New Save";
    public bool updateCurrentSave;
    public SaveMode saveMode = SaveMode.BOTH;

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
                ResponseMessage.SetText($"Save '{save.name}' updated");
            }
            else
            {
                save = await Talo.Saves.CreateSave(saveName, saveMode);
                ResponseMessage.SetText($"Save '{save.name}' created. It will automatically be loaded in next time the scene is loaded.");
            }

        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
