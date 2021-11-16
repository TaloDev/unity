using System;
using UnityEngine;
using UnityEngine.UI;
using TaloGameServices;

public class IdentifyPlayer : MonoBehaviour
{
    public string service, identifier;

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

            GameObject.Find("Panel").GetComponent<Image>().color = new Color(135 / 255f, 1f, 135 / 255f);
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
        }
    }
}
