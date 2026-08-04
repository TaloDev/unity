using UnityEngine;
using System;

namespace TaloGameServices.Sample.Playground
{
    public class SetProp : MonoBehaviour
    {
        public string key, value;

        public void OnButtonClick()
        {
            UpdateProp();
        }

        private async void UpdateProp()
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                ResponseMessage.SetText("key or value not set on SetPropButton");
                return;
            }

            try
            {
                var result = await Talo.CurrentPlayer.SetProp(key, value);

                if (result.RejectedProps.Length > 0)
                {
                    var reasons = string.Join(", ", Array.ConvertAll(result.RejectedProps, (rp) => $"[{rp.key}] {rp.message}"));
                    ResponseMessage.SetText($"Rejected props: {reasons}");
                    return;
                }

                ResponseMessage.SetText($"{key} saved successfully");
            }
            catch (Exception ex)
            {
                ResponseMessage.SetText(ex.Message);
            }
        }
    }
}
