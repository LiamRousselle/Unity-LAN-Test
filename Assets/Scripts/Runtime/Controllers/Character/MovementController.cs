using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MovementController : NetworkBehaviour {

    [Header("Force Configuration")]
    public float WalkSpeed = 5.0f;
    [SerializeField] private float SprintAdditionalSpeed = 1.75f;

    private float SprintSpeed, PreviousSprintSpeed;

    [Header("Physics")]
    [SerializeField] private float MoveAcceleration = 15.0f;
    [SerializeField] private float MoveFriction = 10.0f;

    [SerializeField] private float SprintMomentumGainSpeed = 5.0f;

    private Vector3 MoveVelocity;

    private CharacterController Controller;
    private ControlInputs Inputs;

    [Header("Stamina Settings")]
    [SerializeField] private float MaxStamina = 100.0f;
    [SerializeField] private float StaminaLossSpeed = 15.0f;
    [SerializeField] private float StaminaRegenSpeed = 20.0f;
    [SerializeField] private float StaminaCooldownBeforeRegen = 1.5f;

    public float Stamina;
    private float StaminaCooldown;

    [Header("Events")]
    public UnityEvent OnWalkingBegan;
    public UnityEvent OnWalkingEnded;

    public UnityEvent OnSprintingBegan;
    public UnityEvent OnSprintingEnded;

    private bool WasWalking, WasSprinting;

    // Called to change the walkspeed to the provided walkspeed
    [QFSW.QC.Command]
    public void SetWalkSpeed(float Speed) { WalkSpeed = Speed; }

    // Called when the Player is spawned into the game world
    public override void OnStartLocalPlayer() {
        // If we don't have network ownership over this object
        // return out of this statement
        if (!isLocalPlayer) { return; }

        // Get the inputs
        Inputs = new ControlInputs();
        Inputs.Enable();
    }

    // Called to retrieve the Movement direction
    // from the RawMovementDirection
    private Vector3 GetMovementDirection(Vector2 RawMovementDirection) {
        return 
            (transform.forward * RawMovementDirection.y) + 
            (transform.right * RawMovementDirection.x);
    }

    // Called when the movement should be updated
    private void UpdateMovement() {
        // Get the movement directions
        Vector2 RawMovementVector = Inputs.Character.Movement.ReadValue<Vector2>();
        Vector3 MoveDirection = GetMovementDirection(RawMovementVector) * WalkSpeed;

        // Callbacks
        bool IsMoving = RawMovementVector.magnitude > 0.1;
        if (IsMoving != WasWalking) {
            if (IsMoving) { OnWalkingBegan.Invoke(); }
            else { OnWalkingEnded.Invoke(); }
        }

        // Apply movement physics
        float PhysicsSpeed = (RawMovementVector.magnitude > 0.1 ? MoveAcceleration : MoveFriction) * Time.deltaTime;
        MoveVelocity = Vector3.Lerp(MoveVelocity, MoveDirection, Mathf.Min(PhysicsSpeed, 1.0f));

        // Move the controller
        Controller.Move(MoveVelocity * Time.deltaTime);

        // Set was walking variable
        WasWalking = IsMoving;
    }
    
    // Called to determine if we should sprint
    private bool CheckIfShouldSprinting() {
        if (MoveVelocity.magnitude <= 0.75f) { return false; }
        if (Stamina <= 0.0f) { return false; }

        return Inputs.Character.Sprint.ReadValue<float>() > 0.1f;
    }

    // Called to update stamina
    private void UpdateStamina(bool IsSprinting) {
        // If we are sprinting and we have enough stamina then take stamina away
        if (IsSprinting && Stamina > 0.0f) {
            StaminaCooldown = Time.time + StaminaCooldownBeforeRegen;
            Stamina -= Mathf.Min(Time.deltaTime * StaminaLossSpeed, 1.0f);
        }

        // Otherwise if we are not on cooldown regen stamina
        else if (StaminaCooldown <= Time.time) {
            Stamina = Mathf.Clamp(Stamina + (Time.deltaTime * StaminaRegenSpeed), 0.0f, MaxStamina);
        }
    }

    // Called when sprinting should be updated
    private void UpdateSprinting() {
        // Check if we should be sprinting
        bool IsSprinting = CheckIfShouldSprinting();

        // Update the stamina
        UpdateStamina(IsSprinting);

        // Signal that the sprint state is changing
        if (IsSprinting != WasSprinting) {
            if (IsSprinting) { OnSprintingBegan.Invoke(); }
            else { OnSprintingEnded.Invoke(); }
        }
        WasSprinting = IsSprinting;

        // Do speed stuff man
        float TargetSpeed = IsSprinting ? SprintAdditionalSpeed : 0.0f;
        SprintSpeed = Mathf.Lerp(SprintSpeed, TargetSpeed, Mathf.Min(Time.deltaTime * SprintMomentumGainSpeed, 1.0f));

        // Apply walk speed!!
        WalkSpeed += SprintSpeed - PreviousSprintSpeed;

        // Set the previous speed to the current speed
        PreviousSprintSpeed = SprintSpeed;
    }

    // Called each render step
    private void Update() {
        // If we don't have network ownership over this object
        // return out of this statement
        if (!isLocalPlayer) { return; }

        // Check if we have our input system
        if (Inputs == null) { return; }

        // Update le movement
        UpdateMovement();
        UpdateSprinting();
    }

    // Called when the object is spawned into the world
    private void Awake() {
        // Get the character controller
        Controller = GetComponent<CharacterController>();

        // Set the stamina to the max stamina
        Stamina = MaxStamina;
    }

}
