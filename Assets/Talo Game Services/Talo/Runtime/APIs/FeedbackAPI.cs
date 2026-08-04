using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace TaloGameServices
{
    public class FeedbackAPI : BaseAPI
    {
        public FeedbackAPI() : base("v1/game-feedback") { }

        public async Task<FeedbackCategory[]> GetCategories()
        {
            var uri = new Uri($"{baseUrl}/categories");
            var json = await Call(uri, "GET");

            var res = JsonUtility.FromJson<FeedbackCategoriesResponse>(json);
            return res.feedbackCategories;
        }

        public async Task<FeedbackSendResult> Send(string categoryInternalName, string comment, params (string, string)[] props)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/categories/{categoryInternalName}");
            var propsArray = props.Select((propTuples) => new Prop(propTuples)).ToArray();
            var content = JsonUtility.ToJson(new FeedbackPostRequest { comment = comment, props = propsArray });

            try
            {
                await Call(uri, "POST", content);
                return new FeedbackSendResult(true);
            }
            catch (RequestException ex)
            {
                if (ex.IsBadRequest())
                {
                    return new FeedbackSendResult(false, RejectedProp.FromJson(ex.responseBody));
                }
                throw;
            }
        }

        public class FeedbackSendResult
        {
            public bool Success { get; }
            public RejectedProp[] RejectedProps { get; }

            public FeedbackSendResult(bool success, RejectedProp[] rejectedProps = null)
            {
                Success = success;
                RejectedProps = rejectedProps ?? Array.Empty<RejectedProp>();
            }
        }
    }
}
