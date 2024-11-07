using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGuiManager : MonoBehaviour, IReferredPlayer {

    [Header("References")]
    [SerializeField] private InteractionGuiManager InteractionGui;
    [SerializeField] private Crosshair CrosshairController;
    [SerializeField] private StatusBarManager StatusBar;

    private Player LocalPlayer;

    // Called when the Player data is submitted
    public void OnDataSubmitted(Player playerRef) {
        // Set the local player variable
        LocalPlayer = playerRef;

        // Initialize interaction GUI
        InteractionGui.Initialize(playerRef);

        // Initialize crosshair GUI
        CrosshairController.Initialize(playerRef);

        // Initialize status bars
        StatusBar.InitializeAllStatusBars(playerRef);
    }

}
