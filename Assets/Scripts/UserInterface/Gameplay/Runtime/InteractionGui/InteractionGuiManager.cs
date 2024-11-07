using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionGuiManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TextMeshProUGUI ActionLabel;
    [SerializeField] private Transform Crosshair;

    [SerializeField] private GameObject HoldSliderPrefab;

    private Crosshair CrosshairModule;
    private GameObject HoldDurationObject;
    private Slider HoldSlider;

    private string OriginActionText;

    private Player LocalPlayer;
    private InteractionManager Interaction;

    private bool PrevCanDisplay;

    private string PROTOTYPE_INTERACT_KEY_DISPLAY = "E";

    private Vector3 ActionLabelHiddenPosition, ActionLabelShownPosition;
    private Vector3 ActionLabelScale;

    private Color ActionLabelHiddenColor = new Color(1, 1, 1, 0);
    private Color ActionLabelShownColor;

    private IEnumerator HoldSliderSuccessTransitionOut(Slider Slider, GameObject Object) {
        // Scale the object
        LeanTween.scale(Object, Object.transform.localScale * 1.25f, 0.25f).setEaseOutQuad();
        
        // Find the background and hide it
        // Also find the other thing
        foreach (Transform Result in Object.GetComponentsInChildren<Transform>()) {
            switch (Result.name) {
                case "Outer":
                    Destroy(Result.gameObject);
                    break;
                case "Fill":
                    Image bImage = Result.GetComponent<Image>();
                    LeanTween.value(Result.gameObject, 0.0f, 1.0f, 0.5f).setEaseOutQuad().setOnUpdate((float Influence) => {
                        float Opacity = 1 - Influence;
                        bImage.color = new Color(bImage.color.r, bImage.color.g, bImage.color.b, Opacity);
                    });

                    break;
            }
        }

        // Wait a second then destroy the object
        yield return new WaitForSeconds(0.5f);
        Destroy(Object);
    }

    // Called when the MainGuiManager tell this manager to initialize
    public void Initialize(Player bPlayer) {
        LocalPlayer = bPlayer;
        Interaction = bPlayer.Interaction;
    }

    // Called when we want to show the ActionLabel
    private void DisplayInteractGui() {
        // Show the action label
        string InputTypeName = Interaction.HoveredSettings.HoldDuration > 0.0f ? "Hold" : "Press";
        ActionLabel.text = string.Format(OriginActionText, InputTypeName, PROTOTYPE_INTERACT_KEY_DISPLAY, Interaction.HoveredSettings.ActionText);

        // Lerp color & position
        ActionLabel.color = Color.Lerp(ActionLabel.color, ActionLabelShownColor, Mathf.Min(Time.deltaTime * 10.0f, 1.0f));

        // Change crosshair state
        CrosshairModule.State = CrosshairState.Active;

        // Show the hold duration circle if neccessary
        if (Interaction.HoveredSettings.HoldDuration > 0.0f) {
            HoldDurationObject = Instantiate(HoldSliderPrefab);
            HoldDurationObject.transform.SetParent(Crosshair, false);
            HoldDurationObject.transform.localPosition = Vector3.zero;

            HoldSlider = HoldDurationObject.GetComponent<Slider>();
        }
    }

    // Called when we want to hide the ActionLabel
    private void HideInteractGui() {
        // Change crosshair state
        CrosshairModule.State = CrosshairState.Inactive;

        // Remove the hold slider
        if (HoldDurationObject) {
            // If we triggered something then do the cool animation
            if (Interaction.HoveredSettings != null && Interaction.HoveredSettings.HoldDuration <= Interaction.HeldTime) {
                IEnumerator Corountine = HoldSliderSuccessTransitionOut(HoldSlider, HoldDurationObject);
                StartCoroutine(Corountine);

                HoldDurationObject = null;
                HoldSlider = null;
            }

            // Otherwise do the boring animation
            else {
                Destroy(HoldDurationObject);

                HoldDurationObject = null;
                HoldSlider = null;
            }
        }
    }

    // Called to update the action text
    private void UpdateActionText(bool IsShown) {
        Color WishColor = IsShown ? ActionLabelShownColor : ActionLabelHiddenColor;
        Vector3 WishPosition = IsShown ? ActionLabelShownPosition : ActionLabelHiddenPosition;
        Vector3 WishScale = IsShown ? ActionLabelScale : Vector3.one * 0.1f;

        // Get the lerp time interval
        float TimeInterval = Mathf.Min(Time.deltaTime * 10.0f, 1.0f);

        // Lerp color & position & scale
        ActionLabel.color = Color.Lerp(ActionLabel.color, WishColor, TimeInterval);
        ActionLabel.transform.localPosition = Vector3.Lerp(ActionLabel.transform.localPosition, WishPosition, TimeInterval);
        ActionLabel.transform.localScale = Vector3.Lerp(ActionLabel.transform.localScale, WishScale, TimeInterval);
    }

    // Called to update the hold slider
    private void UpdateHoldSlider() {
        if (Interaction.HoveredSettings == null) { return; }
        if (HoldSlider == null) { return; }

        HoldSlider.value = Interaction.HeldTime / Interaction.HoveredSettings.HoldDuration;
    }

    // Called every renderstep
    private void Update() {
        bool CanDisplay = Interaction.HoveredSettings != null && Interaction.TriggerCooldown <= Time.time;

        // Update the hold slider
        UpdateHoldSlider();

        // Update the action text
        UpdateActionText(CanDisplay);

        // jikosdaknljasdkjlsdajklsdadsajkldsajkl
        if (CanDisplay != PrevCanDisplay) {
            if (CanDisplay) { DisplayInteractGui(); }
            else { HideInteractGui(); }
        }

        PrevCanDisplay = CanDisplay;
    }

    // Called when this object is initialized
    private void Awake() {
        CrosshairModule = Crosshair.GetComponent<Crosshair>();
        OriginActionText = ActionLabel.text;

        // Get the action label positions
        ActionLabelShownPosition = ActionLabel.transform.localPosition;
        ActionLabelHiddenPosition = ActionLabelShownPosition - Vector3.up * 25;

        // Get the action label scale
        ActionLabelScale = ActionLabel.transform.localScale;

        // Get the default action label color
        ActionLabelShownColor = ActionLabel.color;

        // Change action label color & position & size
        ActionLabel.color = ActionLabelHiddenColor;
        ActionLabel.transform.localPosition = ActionLabelHiddenPosition;
        ActionLabel.transform.localScale = Vector3.one * 0.1f;
    }

}
