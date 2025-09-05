using System;
using UnityEngine;

namespace TaloGameServices.Sample.Playground
{
    public class GetStat : MonoBehaviour
    {
        public string statInternalName;

        public void OnButtonClick()
        {
            FetchStat();
        }

        private async void FetchStat()
        {
            if (string.IsNullOrEmpty(statInternalName))
            {
                ResponseMessage.SetText("statInternalName not set on GetStatButton");
                return;
            }

            try
            {
                var res = await Talo.Stats.Find(statInternalName);
                ResponseMessage.SetText($"{res.name} is{(res.global ? "" : " not")} a global stat, with a default value of {res.defaultValue}");
            }
            catch (Exception ex)
            {
                ResponseMessage.SetText(ex.Message);
                throw;
            }
        }
    }
}
