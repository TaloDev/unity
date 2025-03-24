using System;
using UnityEngine;
using TaloGameServices;

public class GetGlobalHistory : MonoBehaviour
{
    public string statInternalName;

    public void OnButtonClick()
    {
        FetchGlobalHistory();
    }

    private async void FetchGlobalHistory()
    {
        if (string.IsNullOrEmpty(statInternalName))
        {
            ResponseMessage.SetText("statInternalName not set on GetGlobalHistoryButton");
            return;
        }

        try
        {
            var res = await Talo.Stats.GetGlobalHistory(statInternalName);

            ResponseMessage.SetText($"Min: {res.globalValue.minValue}, max: {res.globalValue.maxValue}, median: {res.globalValue.medianValue}, average: {res.globalValue.averageValue}, average change: {res.globalValue.averageChange}");
        }
        catch (Exception err)
        {
            ResponseMessage.SetText(err.Message);
            throw err;
        }
    }
}
