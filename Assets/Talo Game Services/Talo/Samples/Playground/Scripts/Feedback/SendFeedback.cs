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
                await Talo.Feedback.Send(categoryInternalName, feedbackComment);
                ResponseMessage.SetText($"Feedback sent for {categoryInternalName}: {feedbackComment}");
            }
            catch (Exception err)
            {
                ResponseMessage.SetText(err.Message);
                throw err;
            }
        }
    }
}
