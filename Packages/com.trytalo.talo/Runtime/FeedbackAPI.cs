using System;
using System.Threading.Tasks;
using UnityEngine;

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

        public async Task Send(string internalName, string comment)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/categories/{internalName}");
            var content = JsonUtility.ToJson(new FeedbackPostRequest { comment = comment });

            await Call(uri, "POST", content);
        }
    }
}
