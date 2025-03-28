using System;
using System.Linq;
using UnityEngine;

namespace TaloGameServices.Sample.Playground
{
    public class GetAllStats : MonoBehaviour
    {
        public void OnButtonClick()
        {
            FetchStats();
        }

        private async void FetchStats()
        {
            try
            {
                var res = await Talo.Stats.GetStats();
                var internalNames = res.Length > 0 ? string.Join(", ", res.Select((item) => item.internalName)) : "no stats";
                ResponseMessage.SetText($"Stats: {internalNames}");
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
