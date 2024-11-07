using Mirror;
using TMPro;
using System;
using UnityEngine;

public class PingTracker : MonoBehaviour {

    private TextMeshProUGUI Label;
    private string OriginText;
    private float Cooldown;

    private void Awake() {
        Label = GetComponent<TextMeshProUGUI>();
        OriginText = Label.text;
    }

    private void Update() {
        if (Cooldown > Time.time) { return; }

        Label.text = string.Format(OriginText, Math.Round(NetworkTime.rtt * 1000).ToString());
        Cooldown = Time.time + 0.25f;
    }

}
