using UnityEngine;

public class OpenDocs : MonoBehaviour
{
    public void OnButtonClick()
    {
        Application.OpenURL("https://docs.trytalo.com/docs/unity/install?utm_source=unity-playground");
    }
}
