using System;
using UnityEngine;
using UnityEngine.UI;
using TaloGameServices;

public class IdentifyPlayer : MonoBehaviour
{
    public string service, identifier;

    private void Start()
    {
        Talo.Saves.OnSavesLoaded += () =>
        {
            if (Talo.Saves.All.Length > 0)
            {
                Talo.Saves.ChooseSave(Talo.Saves.Latest);
            }
        };
    }

    public void OnButtonClick()
    {
        Identify();
    }

    private async void Identify()
    {
        try
        {
            await Talo.Players.Identify(service, identifier);
            ResponseMessage.SetText("Identified!");

            var panel = GameObject.Find("Panel");
            if (panel != null)
            {
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
