using System;
using UnityEngine;

namespace TaloGameServices.Sample.Playground
{
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
                var globalMetrics = res.globalValue;
                var playerMetrics = res.playerValue;

                ResponseMessage.SetText(
                    $"Min: {globalMetrics.minValue}, " +
                    $"max: {globalMetrics.maxValue}, " +
                    $"median: {globalMetrics.medianValue}, " +
                    $"average: {globalMetrics.averageValue}, " +
                    $"average change: {globalMetrics.averageChange}, " +
                    $"average player value: {playerMetrics.averageValue}"
                );
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
