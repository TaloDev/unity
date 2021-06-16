using UnityEngine;
using TaloGameServices;
using System;

public class TrackLevelUpEvent : MonoBehaviour {
    public int level = 1;
    private float timeTaken;

    public void OnButtonClick() {
        Track();
    }

    private void Track() {
        level++;

        try {
            Talo.Events.Track(
                "Levelled up",
                ("newLevel", level.ToString()),
                ("timeTaken", timeTaken.ToString())
            );

            ResponseMessage.SetText("Levelled up tracked");
        } catch (Exception err) {
            ResponseMessage.SetText(err.Message);
        }

        timeTaken = 0;
    }

    private void Update() {
        timeTaken += Time.deltaTime;
    }
}
