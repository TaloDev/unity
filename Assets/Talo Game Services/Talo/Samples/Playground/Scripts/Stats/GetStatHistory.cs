using System;
using UnityEngine;
using System.Linq;

namespace TaloGameServices.Sample.Playground
{
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
                var changeString = res.count > 0 ? string.Join(", ", res.history.Select((item) => item.change)) : "no changes";

                ResponseMessage.SetText($"{statInternalName} changed by: {changeString}");
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
