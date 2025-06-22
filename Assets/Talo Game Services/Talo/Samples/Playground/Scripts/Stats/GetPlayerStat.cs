using System;
using UnityEngine;

namespace TaloGameServices.Sample.Playground
{
    public class GetPlayerStat : MonoBehaviour
    {
        public string statInternalName;

        public void OnButtonClick()
        {
            FetchPlayerStat();
        }

        private async void FetchPlayerStat()
        {
            if (string.IsNullOrEmpty(statInternalName))
            {
                ResponseMessage.SetText("statInternalName not set on GetPlayerStatButton");
                return;
            }

            try
            {
                var res = await Talo.Stats.FindPlayerStat(statInternalName);
                ResponseMessage.SetText($"{statInternalName} value: {(res == null ? "not set" : res.value)}, last updated: {res?.updatedAt ?? "never"}");
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
