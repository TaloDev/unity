using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TaloGameServices;

public class SetHealthProp : MonoBehaviour
{
    public string key = "currentHealth";

    public void OnButtonClick()
    {
        UpdateProp();
    }

    private void UpdateProp()
    {
        string value = Random.Range(0, 100).ToString();

        Talo.CurrentPlayer.SetProp(key, value);
        ResponseMessage.SetText($"{key} set to {value}");
    }
}
