using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResponseMessage : MonoBehaviour {
    public static void SetText(string text) {
        GameObject.Find("Response Message").GetComponent<Text>().text = $"{DateTime.Now.TimeOfDay}: {text}";
    }
}
