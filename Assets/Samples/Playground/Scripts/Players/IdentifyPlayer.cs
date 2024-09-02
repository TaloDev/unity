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
        Talo.Players.OnIdentified += OnIdentified;
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
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }

    private void OnIdentified(Player player)
    {
        var panel = GameObject.Find("Panel");
        if (panel != null)
        {
            ResponseMessage.SetText("Identified!");
            panel.GetComponent<Image>().color = new Color(135 / 255f, 1f, 135 / 255f);
        }
    }
}
