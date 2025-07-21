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

        public async Task Send(string categoryInternalName, string comment, params (string, string)[] props)
        {
            Talo.IdentityCheck();

            var uri = new Uri($"{baseUrl}/categories/{categoryInternalName}");
            var propsArray = props.Select((propTuples) => new Prop(propTuples)).ToArray();
            var content = JsonUtility.ToJson(new FeedbackPostRequest { comment = comment, props = propsArray });

            await Call(uri, "POST", content);
        }
    }
}
