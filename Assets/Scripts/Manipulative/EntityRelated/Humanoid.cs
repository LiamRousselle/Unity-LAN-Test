using Mirror;
using UnityEngine;

public class Humanoid : NetworkBehaviour {

    [Header("Health Settings")]
    [SyncVar] public float Health = 100.0f;
    [SyncVar] public float MaxHealth = 100.0f;

}
