using System;
using UnityEngine;
using UnityEngine.UI;
using TaloGameServices;
using System.Threading.Tasks;

public class IdentifyPlayer : MonoBehaviour
{
    public string service, identifier;

    private void Start()
    {
        Talo.Players.OnIdentified += async (player) =>
        {
            await Talo.Saves.GetSaves();
        };

        Talo.Saves.OnSavesLoaded += () =>
        {
            if (Talo.Saves.All.Length > 0)
            {
                ResponseMessage.SetText("Loading...");
                Talo.Saves.ChooseSave(Talo.Saves.Latest);
            } else
            {
                ResponseMessage.SetText("No saves found. Modify the scene and then create one.");
                GameObject.Find("Overlay")?.SetActive(false);
            }
        };

        Talo.Saves.OnSaveLoadingCompleted += () =>
        {
            ResponseMessage.SetText($"Save '{Talo.Saves.Current.name}' loaded");
            GameObject.Find("Overlay")?.SetActive(false);
        };
    }

    public async void OnButtonClick()
    {
        await Identify();
    }

    private async Task Identify()
    {
        try
        {
            await Talo.Players.Identify(service, identifier);

            var panel = GameObject.Find("Panel");
            if (panel != null)
            {
                ResponseMessage.SetText("Identified!");
                panel.GetComponent<Image>().color = new Color(135 / 255f, 1f, 135 / 255f);
            }
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
