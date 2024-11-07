using UnityEngine;

public class DoorBehaviour : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject CollisionObject;

    [Header("Values")]
    public bool IsOpened = false;
    private float DoorOpenRange = 2.0f;

    // Called to check if the door should be opened
    private bool ShouldDoorBeOpened() {
        // Get all colliders in our provided radius
        Collider[] FoundColliders = Physics.OverlapSphere(transform.position, DoorOpenRange);
        foreach (Collider Result in FoundColliders) {
            if (Vector3.Distance(transform.position, Result.transform.position) <= DoorOpenRange) {
                // Make sure we have a humanoid attached
                if (Result.gameObject.GetComponent<Humanoid>() != null) {
                    return true;
                }
            }
        }
    
        return false;
    }

    // Called to change the current door state
    private void ChangeDoorState() {
        CollisionObject.SetActive(!IsOpened);
    }

    // Called every physics renderstep
    private void FixedUpdate() {
        IsOpened = ShouldDoorBeOpened();
        ChangeDoorState();
    }

}
