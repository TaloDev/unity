using System;
using UnityEngine;

namespace TaloGameServices.Sample.Playground
{
    public class TrackStat : MonoBehaviour
    {
        public string statInternalName;
        public float change = 1;

        public void OnButtonClick()
        {
            Track();
        }

        private async void Track()
        {
            if (string.IsNullOrEmpty(statInternalName))
            {
                ResponseMessage.SetText("statInternalName not set on TrackStatButton");
                return;
            }

            try
            {
                var res = await Talo.Stats.Track(statInternalName, change);

                ResponseMessage.SetText($"{statInternalName} changed by {change}, new value is {res.playerStat.value}");
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
