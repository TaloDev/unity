using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace TaloGameServices.Sample.Playground
{

    public class GetCategories : MonoBehaviour
    {
        public async void OnButtonClick()
        {
            await FetchCategories();
        }

        private async Task FetchCategories()
        {
            var categories = await Talo.Feedback.GetCategories();

            if (categories.Length == 0)
            {
                ResponseMessage.SetText("No categories found. Create some in the Talo dashboard!");
            }
            else
            {
                var mapped = categories.Select((c) => $"{c.name} ({c.internalName})");
                ResponseMessage.SetText($"Categories: " + string.Join(", ", mapped));
            }
        }
    }
}
