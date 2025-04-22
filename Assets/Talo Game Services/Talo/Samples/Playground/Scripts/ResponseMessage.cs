using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaloGameServices.Sample.Playground
{
    public class ResponseMessage : MonoBehaviour
    {
        public static void SetText(string text)
        {
            GameObject.Find("Response Message").GetComponent<Text>().text = $"{DateTime.Now.TimeOfDay}: {text}";
        }
    }
}
