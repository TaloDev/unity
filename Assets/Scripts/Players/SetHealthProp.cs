using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TaloGameServices;
using System.Threading.Tasks;

public class SetHealthProp : MonoBehaviour
{
    public string key = "currentHealth";

    public async void OnButtonClick()
    {
        await UpdateProp();
    }

    private async Task UpdateProp()
    {
        string value = Random.Range(0, 100).ToString();

        await Talo.CurrentPlayer.SetProp(key, value);
        ResponseMessage.SetText($"{key} set to {value}");
    }
}
