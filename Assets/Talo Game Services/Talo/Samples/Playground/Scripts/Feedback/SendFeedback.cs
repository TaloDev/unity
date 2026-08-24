using UnityEngine;
using System;

namespace TaloGameServices.Sample.Playground
{
    public class SendFeedback : MonoBehaviour
    {
        public string categoryInternalName, feedbackComment;

        public async void OnButtonClick()
        {
            if (string.IsNullOrEmpty(categoryInternalName) || string.IsNullOrEmpty(feedbackComment))
            {
                ResponseMessage.SetText("categoryInternalName or feedbackComment not set on SendFeedbackButton");
                return;
            }

            try
            {
                var result = await Talo.Feedback.Send(categoryInternalName, feedbackComment);
                if (result.Success)
                {
                    ResponseMessage.SetText($"Feedback sent for {categoryInternalName}: {feedbackComment}");
                }
                else
                {
                    ResponseMessage.SetText("Failed to send feedback");
                }
            }
            catch (Exception ex)
            {
                ResponseMessage.SetText(ex.Message);
                throw;
            }
        }
    }
}
