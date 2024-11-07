using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable {
    [Command(requiresAuthority = false)]
    public void Triggered();
}

public class InteractionManager : NetworkBehaviour {

    [Header("References")]
    [SerializeField] private Transform Camera;

    private Player LocalPlayer;
    private ControlInputs Inputs;

    [Header("Default Distance Range")]
    [SerializeField, Range(0.0f, 1000.0f)] private float Range = 100.0f;

    [Header("Whitelisted Objects")]
    [SerializeField] private LayerMask Whitelist;

    [Header("Shared")]
    public InteractSettings HoveredSettings;
    public Interactable HoveredInteractable;
    public float HeldTime = 0.0f;

    private bool WasJustTriggered;
    public float TriggerCooldown = -1.0f;

    // Called when the Player is spawned into the game world
    public override void OnStartLocalPlayer() {
        // If we don't have network ownership over this object
        // return out of this statement
        if (!isLocalPlayer) { return; }

        // Get the localplayer object
        LocalPlayer = GetComponent<Player>();

        // Get the input class
        Inputs = new ControlInputs();
        Inputs.Enable();
    }

    // Called to check if the player is pressing the interact key
    private bool IsInteractKeyDown() {
        return Inputs.Character.Interaction.ReadValue<float>() > 0.1f;
    }

    // Called to find a object with an interactable interface
    private Interactable GetInteractable(Vector3 Origin) {
        Vector3 Direction = Camera.forward.normalized;

        RaycastHit Result;
        if (Physics.Raycast(Origin, Direction, out Result, Range, Whitelist)) {
            // Check that it has a Interactable interface attached to it
            if (Result.collider.TryGetComponent(out Interactable FoundInterface)) {
                // Check if there are interact settings on the same component
                InteractSettings FoundSettings = Result.collider.GetComponent<InteractSettings>();

                // If there are no settings found just return the interface and do nothing else
                if (FoundSettings == null) {
                    HoveredSettings = null;
                    return FoundInterface; 
                }

                // Do a distance check
                if (Vector3.Distance(Camera.position, Result.collider.transform.position) <= FoundSettings.MaxDistance) {
                    // If we're in range then set the hoveredsettings and return our interface
                    HoveredSettings = FoundSettings;
                    return FoundInterface;
                }
                else { HoveredSettings = null; }
            }
        }

        HoveredSettings = null;
        return null;
    }

    // Called to trigger an interactable from a reference Interactable interface
    private void TriggerInteractableFromReference(Interactable Reference) {
        WasJustTriggered = true;
        TriggerCooldown = Time.time + 0.4f;

        Reference.Triggered();
    }

    // Called to reset the hold timer
    private void ResetHoldTimer() {
        HeldTime = 0.0f;
    }

    // Called every physics renderstep
    private void FixedUpdate() {
        // Make sure we own this interaction manager
        if (!isLocalPlayer) { return; }

        // Make sure our input system is loaded
        if (Inputs == null) { return; }

        // Get our interactable reference
        Vector3 Origin = Camera.position;
        HoveredInteractable = GetInteractable(Origin);
    }

    // Called every renderstep
    private void Update() {
        // Make sure we own this interaction manager
        if (!isLocalPlayer) { return; }

        // Make sure our input system is loaded
        if (Inputs == null) { return; }

        // Make sure our reference exists
        if (HoveredInteractable != null) {
            bool IsKeyDown = IsInteractKeyDown();

            // If we just triggered a object we don't want to spam trigger that same object
            if (WasJustTriggered || TriggerCooldown > Time.time) {
                WasJustTriggered = IsKeyDown;
                ResetHoldTimer();

                return;
            }

            // If we are not pressing the interact key don't proceed
            if (!IsKeyDown) { ResetHoldTimer(); return; }

            // If we don't have a InteractSettings found then trigger the interface
            if (HoveredSettings == null) {
                TriggerInteractableFromReference(HoveredInteractable);
            }

            // Otherwise if we do have an InteractSettings then do the stuff below
            else {
                // If there is no HoldDuration then trigger normally
                if (HoveredSettings.HoldDuration <= 0.0f) {
                    TriggerInteractableFromReference(HoveredInteractable);
                }

                // Otherwise increase the hold timer until we reach our goal
                else {
                    HeldTime += Time.deltaTime;
                    if (HeldTime >= HoveredSettings.HoldDuration) {
                        TriggerInteractableFromReference(HoveredInteractable);
                    }
                }
            }

        }

        // Otherwise if there is no interactable which is being hovered reset the hold time
        // And set WasJustTriggered to true so it thinks the user lifted there finger off the interact key :)
        // What a lovely hack
        else { ResetHoldTimer(); WasJustTriggered = true; }
    }

}
