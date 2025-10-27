using System;
using System.Linq;
using UnityEngine;

namespace TaloGameServices.Sample.Playground
{
    public class ListPlayerStats : MonoBehaviour
    {
        public void OnButtonClick()
        {
            FetchPlayerStats();
        }

        private async void FetchPlayerStats()
        {
            try
            {
                var res = await Talo.Stats.ListPlayerStats();
                var statsList = res.Length > 0
                    ? string.Join(", ", res.Select((item) => $"{item.stat.internalName} = {item.value}"))
                    : "none";

                ResponseMessage.SetText($"Player stats: {statsList}");
            }
            catch (Exception ex)
            {
                ResponseMessage.SetText(ex.Message);
                throw;
            }
        }
    }
}
