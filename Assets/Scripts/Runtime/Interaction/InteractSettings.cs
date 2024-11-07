using UnityEngine;
using UnityEngine.Scripting;

[RequiredInterface(typeof(Interactable))]
public class InteractSettings : MonoBehaviour {

    [Header("Range")]
    public float MaxDistance = 2.0f;

    [Header("Visual")]
    public string ActionText = "ACTION TEXT STRING WAS NULL OR EMPTY";
    public float HoldDuration = 0.0f;

}
