using System;
using UnityEngine;
using TaloGameServices;
using System.Linq;

public class GetStatHistory : MonoBehaviour
{
    public string statInternalName;

    public void OnButtonClick()
    {
        FetchHistory();
    }

    private async void FetchHistory()
    {
        if (string.IsNullOrEmpty(statInternalName))
        {
            ResponseMessage.SetText("statInternalName not set on GetStatHistoryButton");
            return;
        }

        try
        {
            var res = await Talo.Stats.GetHistory(statInternalName);

            ResponseMessage.SetText($"{statInternalName} changed by: {string.Join(", ", res.history.Select((item) => item.change))}");
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
