using UnityEngine;
using UnityEngine.UI;

public enum CrosshairState {
    Disabled,
    Inactive,
    Active
}

// This class code is very messy! (I don't really care.)
public class Crosshair : MonoBehaviour {

    [Header("References")]
    [SerializeField] private RawImage Dot;
    
    [Header("Color States")]
    [SerializeField] private Color DisabledColor = new Color(1, 1, 1, 0);
    [SerializeField] private Color InactiveColor = new Color(0.77f, 0.77f, 0.77f, 0.5f);
    [SerializeField] private Color ActiveColor = new Color(1, 1, 1, 1);

    [Header("Size States")]
    [SerializeField] private Vector2 DisabledSize = new Vector2(0, 0);
    [SerializeField] private Vector2 InactiveSize = new Vector2(1, 1);
    [SerializeField] private Vector2 ActiveSize = new Vector2(2, 2);

    [Header("Interpolation Speeds")]
    [SerializeField, Range(0.0f, 100.0f)] private float InterpolateSpeed = 15.0f;

    [Header("Current State")]
    public CrosshairState State = CrosshairState.Inactive;

    private Player LocalPlayer;
    private Camera ControlCamera;

    // Called when the MainGuiManager tell this manager to initializ
    public void Initialize(Player bPlayer) {
        // Set the LocalPlayer
        LocalPlayer = bPlayer;

        // Get the control camera
        ControlCamera = LocalPlayer.Camera.Renderer.GetComponent<Camera>();
    }

    // Interpolates the image color to that color
    private void InterpolateToColor(Color Reference) {
        Dot.color = Color.Lerp(Dot.color, Reference, Mathf.Min(Time.deltaTime * InterpolateSpeed, 1.0f));
    }

    // Interpolates the image size to the provided size
    private void InterpolateToSize(Vector2 Reference) {
        Dot.transform.localScale = Vector2.Lerp(Dot.transform.localScale, Reference, Mathf.Min(Time.deltaTime * InterpolateSpeed, 1.0f));
    }

    // Interpolates the crosshair color to "DisabledColor"
    private void InterpolateToDisabled() {
        InterpolateToColor(DisabledColor);
        InterpolateToSize(DisabledSize);
    }

    // Interpolates the crosshair color to "InactiveColor"
    private void InterpolateToInactive() {
        InterpolateToColor(InactiveColor);
        InterpolateToSize(InactiveSize);
    }

    // Interpolates the crosshair color to "ActiveColor"
    private void InterpolateToActive() {
        InterpolateToColor(ActiveColor);
        InterpolateToSize(ActiveSize);
    }

    // Called to center the crosshair to the crosshair location
    private void CenterCrosshairLocation() {
        // If we don't have a LocalPlayer then return
        if (LocalPlayer == null) { return; }

        // TODO: 
        // Make the crosshair location
    }

    // Called every renderstep
    private void Update() {
        // Determine which state we should interpolate to
        switch(State) {
            default:
                InterpolateToDisabled();
                break;
            case CrosshairState.Inactive:
                InterpolateToInactive();
                break;
            case CrosshairState.Active:
                InterpolateToActive();
                break;
        }

        // Center the crosshair
        CenterCrosshairLocation();
    }

}
