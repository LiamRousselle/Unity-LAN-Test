using UnityEngine;

public class StaminaStatusBarLogic : MonoBehaviour, IStatusBar {

    private Player LocalPlayer;

    // Called when the status bar is initialized
    public void OnStatusBarInitialized(Player bPlayer) {
        LocalPlayer = bPlayer;
    }


}
