using System.Collections.Generic;
using UnityEngine;

public interface IStatusBar {
    void OnStatusBarInitialized(Player bPlayer);
}

public class StatusBarManager : MonoBehaviour {

    [SerializeField] private List<GameObject> StatusBars = new List<GameObject>();
    private Player LocalPlayer;

    // Called to initialize all status bars
    public void InitializeAllStatusBars(Player bPlayer) {
        LocalPlayer = bPlayer;

        // Loop through status bar list
        foreach (GameObject Bar in StatusBars) {
            if (Bar.TryGetComponent(out IStatusBar Logic)) {
                // Initialize if it has the IStatusBar interface
                Logic.OnStatusBarInitialized(bPlayer);
            }
        }
    }

}
