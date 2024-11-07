using Mirror;
using System.Collections.Generic;
using UnityEngine;

public interface IReferredPlayer {
    void OnDataSubmitted(Player playerRef);
}

public class Player : NetworkBehaviour {

    [Header("References")]
    public MovementController Movement;
    public CameraController Camera;
    public InteractionManager Interaction;

    [Header("User Interface")]
    [SerializeField] private List<GameObject> StarterUserInterfaces = new List<GameObject>();

    protected List<GameObject> ActiveInterfaces = new List<GameObject>();

    // Called when the local player is initialized
    public override void OnStartLocalPlayer() {
        // If we are not this local player then return out of the statement
        if (!isLocalPlayer) { return; }

        // Initialize all provided starter user interfaces
        SpawnStarterUserInterfaceObjects();
    }

    // Called to get a user interface from name
    public GameObject GetUserInterfaceFromName(string Name) {
        // Check if we can find that interface
        foreach (GameObject Interface in ActiveInterfaces) {
            if (Interface.name == Name) { return Interface; }
        }

        // Oh noes! We couldn't find it! Throw an error NOW!
        Debug.LogError(string.Format("Failed to find UserInterface with name {0}", Name));
        return null;
    }

    // Called to spawn in all the starter user interface objects
    private void SpawnStarterUserInterfaceObjects() {
        foreach (GameObject Interface in StarterUserInterfaces) {
            // Make a new instance and spawn it into the world
            GameObject Result = Instantiate(Interface);
            Result.transform.name = Interface.name;

            // Check if it has the IReferredPlayer interface
            // If so call the "OnDataSubmitted" function and send this class
            if (Result.TryGetComponent(out IReferredPlayer ResultInterface)) {
                ResultInterface.OnDataSubmitted(this);
            }

            // Add it to the active list
            ActiveInterfaces.Add(Result);
        }
    }

    // Called when the object is enabled
    private void OnEnable() {
        // If we do not own this player then return
        if (!isLocalPlayer) { return; }

        // Spawn the user interface objects ahhhhh
        SpawnStarterUserInterfaceObjects();
    }

    // Called when the object is disabled
    private void OnDisable() {
        // If we do not own this player then don't do this stuff lol
        if (!isLocalPlayer) { return; }

        // Delete all active user interfaces
        foreach(GameObject Interface in ActiveInterfaces) {
            Destroy(Interface);
        }

        // Reset active interfaces
        ActiveInterfaces.Clear();
    }

}
