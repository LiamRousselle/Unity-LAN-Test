using UnityEngine;

public class UserInterfaceDebug : MonoBehaviour, IReferredPlayer {

    public void OnDataSubmitted(Player playerRef) {
        Debug.Log("Recieved data that was submitted.");
    }

}
