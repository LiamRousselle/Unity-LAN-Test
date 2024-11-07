using Mirror;
using UnityEngine;

public class InteractionObjTest : NetworkBehaviour, Interactable {

    // Called when the position state is changed
    [ClientRpc]
    private void OnStateChanged(Vector3 Desired) {
        transform.position = Desired;
    }

    // Called when someone interacts with this object
    [Command(requiresAuthority = false)]
    public void Triggered() {
        transform.position += Vector3.up;
        OnStateChanged(transform.position);
    }
}
