using UnityEngine;
using TaloGameServices;

public class SendFeedback : MonoBehaviour
{
    public string internalName, feedbackComment;

    public async void OnButtonClick()
    {
        await Talo.Feedback.Send(internalName, feedbackComment);
        ResponseMessage.SetText($"Feedback sent for {internalName}: {feedbackComment}");
    }
}
