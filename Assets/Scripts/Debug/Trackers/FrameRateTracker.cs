using UnityEngine;
using TMPro;

public class FrameRateTracker : MonoBehaviour {

    private TextMeshProUGUI Label;
    private string OriginText;

    private float Cooldown;

    private void Awake() {
        Label = GetComponent<TextMeshProUGUI>();
        OriginText = Label.text;
    }

    private void Update() {
        if (Cooldown > Time.time) { return; }

        float FrameRate = Mathf.Round(1 / Time.deltaTime);
        Label.text = string.Format(OriginText, FrameRate.ToString());

        Cooldown = Time.time + 1.0f;
    }

}
