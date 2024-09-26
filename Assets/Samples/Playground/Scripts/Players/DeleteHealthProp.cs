using UnityEngine;
using TaloGameServices;
using System.Threading.Tasks;

public class DeleteHealthProp : MonoBehaviour
{
    public string key = "currentHealth";

    public async void OnButtonClick()
    {
        await DeleteProp();
    }

    private async Task DeleteProp()
    {
        await Talo.CurrentPlayer.DeleteProp(key);
        ResponseMessage.SetText($"{key} deleted");
    }
}
