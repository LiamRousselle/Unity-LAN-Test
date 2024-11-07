using Mirror;
using UnityEngine;

public class CameraController : NetworkBehaviour {

    private bool IsWalking = false;
    public void OnWalkingBegan() { IsWalking = true; }
    public void OnWalkingEnded() { IsWalking = false; }

    private bool IsSprinting = false;
    public void OnSprintingBegan() { IsSprinting = true; }
    public void OnSprintingEnded() { IsSprinting = false; }

    [Header("Refernces")]
    public Transform Camera;
    public Transform Renderer;
    public Transform CrosshairLocation;

    [SerializeField] private GameObject FreecamReference;

    private MovementController CMovementController;

    [Header("Clamping")]
    [SerializeField, Range(-180.0f, 0)] private float LookDownLimit = -80.0f;
    [SerializeField, Range(0, 180.0f)] private float LookUpLimit = 80.0f;

    private float CurrentUpAxis = 0f;
    private float PreviousUpAxis = 0f;

    [Header("Look Limiting")]
    public bool LimitHorizontalLook = false;
    [Range(-180.0f, 0)] public float LookLeftLimit = -45.0f;
    [Range(0, 180.0f)] public float LookRightLimit = 45.0f;

    private float CurrentRightAxis = 0f;
    private float PreviousRightAxis = 0f;

    [Header("Camera Movement Motion")]
    [SerializeField] private float CamMotionAmplitude = 1.0f;
    [SerializeField] private float CamMotionFrequency = 8.0f;

    private float CamMotionInfluence = 0.0f;
    private Vector3 PrevMotionPosition;
    private Vector3 PrevMotionRotation;

    [Header("Sprinting Settings")]
    [SerializeField] private float SprintFieldOfViewGain = 5.0f;
    [SerializeField] private float SprintFieldOfViewChangeSpeed = 10.0f;

    private float SprintFovInterval;
    private float CurrSprintFov, PrevSprintFov;

    [Header("Configuration")]
    [SerializeField] private float MouseSensitivity = 0.05f;

    private Camera RendererCamera;

    private ControlInputs Inputs;
    private GameObject Freecam;

    // Called when the Player is spawned into the game world
    public override void OnStartLocalPlayer() {
        // Change the cameras active state depending
        // on whether we own or don't own this controller
        Camera.gameObject.SetActive(isLocalPlayer);

        // If we don't have network ownership over this object
        // return out of this statement
        if (!isLocalPlayer) { return; }
        
        // Lock & hide the mouse
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Easing Type: Quad
    // Easing Direction: In Out
    public static float EaseInOutQuint(float x) {
        return x < 0.5f ? 16f * x * x * x * x * x : 1 - Mathf.Pow(-2f * x + 2f, 5f) / 2f;
    }

    // Called to rotate the camera & player body normally
    private void ApplyNormalRotation(Vector2 Delta) {
        // TODO: (do this when you almost finish the ladders)

        // if we stopped limiting the camera rotation then
        // rotate the characters y axis to face the same as
        // the cameras y axis. then make the cameras y axis zero
        // Camera.Rotate(Vector3.up * PreviousRightAxis);
        // transform.Rotate(Vector3.up * CurrentRightAxis);

        CurrentRightAxis = 0.0f;

        // Rotate the character
        transform.Rotate(Vector3.up * Delta.x);

        // Rotate the camera
        CurrentUpAxis = Mathf.Clamp(CurrentUpAxis + Delta.y, LookDownLimit, LookUpLimit);

        float OffsetX = CurrentUpAxis - PreviousUpAxis;
        Camera.eulerAngles -= Vector3.right * OffsetX;

        // Set previous variable
        PreviousUpAxis = CurrentUpAxis;
        PreviousRightAxis = 0.0f;
    }

    // Called to rotate the camera & player body at a limited angle
    private void ApplyLimitedRotation(Vector2 Delta) {
        // Get the cameras angles to rotate to
        CurrentRightAxis = Mathf.Clamp(CurrentRightAxis + Delta.x, LookLeftLimit, LookRightLimit);
        CurrentUpAxis = Mathf.Clamp(CurrentUpAxis + Delta.y, LookDownLimit, LookUpLimit);
       
        // Rotate the camera ughhh
        Camera.localEulerAngles = (Vector3.up * CurrentRightAxis) + (Vector3.right * CurrentUpAxis);

        // Set previous variable
        PreviousUpAxis = CurrentUpAxis;
        PreviousRightAxis = CurrentRightAxis;
    }

    // Called each frame and used to calculate the amount
    // we should rotate the camera by depending on the mouse delta
    private void UpdateMouseRotation() {
        // Get the mouse delta
        Vector2 RawDelta = Inputs.Character.Looking.ReadValue<Vector2>();
        Vector2 Delta = RawDelta * MouseSensitivity;

        // Decide rotating method

        // Warning!
        // ApplyLimitedRotation is currently not implemented
        // as a working aspect of the game

        if (!LimitHorizontalLook) { ApplyNormalRotation(Delta); }
        //else { ApplyLimitedRotation(Delta); }
    }

    // Called to get the next Camera motion position
    private Vector3 GetCameraMotionPosition() {
        Vector3 Result = Vector3.zero;
        Result.x += Mathf.Sin(Time.time * CamMotionFrequency) * CamMotionAmplitude;
        Result.y -= (Mathf.Cos(Time.time * CamMotionFrequency * 2) + 1) * CamMotionAmplitude * 0.5f;

        return Result * CamMotionInfluence;
    }

    // Called to get the next camera motion rotation
    private Vector3 GetCameraMotionRotation() {
        Vector3 Result = Vector3.forward * (Mathf.Sin(Time.time * CamMotionFrequency) * CamMotionAmplitude * 5.0f);
        return Result * CamMotionInfluence;
    }

    // Called each frame and used to update the cameras motion
    private void UpdateCameraMotion() {
        // Set influence
        CamMotionInfluence = Mathf.Lerp(CamMotionInfluence, IsWalking ? 1.0f : 0.0f, Mathf.Min(Time.deltaTime * 10.0f, 1.0f));

        // Offset position (must be done before rotation or bugs)
        Vector3 WishPosition = GetCameraMotionPosition();
        Renderer.localPosition += WishPosition - PrevMotionPosition;

        // Offset rotation
        Vector3 WishRotation = GetCameraMotionRotation();
        Renderer.localEulerAngles += WishRotation - PrevMotionRotation;

        // Set previous positions
        PrevMotionPosition = WishPosition;
        PrevMotionRotation = WishRotation;
    }

    // Called to change the field of view depending on the sprinting state
    private void UpdateSprintingFieldOfView() {
        // Get the target fov
        float TargetFov = IsSprinting ? SprintFieldOfViewGain : 0.0f;

        // Get the Fov interval
        float Interval = IsSprinting ? SprintFieldOfViewChangeSpeed : -SprintFieldOfViewChangeSpeed;
        SprintFovInterval = Mathf.Clamp(SprintFovInterval + (Time.deltaTime * Interval), 0.0f, 1.0f);

        // Lerp the influence to that interval
        float LerpInterval = Mathf.Min(Time.deltaTime * 4.0f, 1.0f);
        CurrSprintFov = Mathf.Lerp(CurrSprintFov, EaseInOutQuint(SprintFovInterval) * TargetFov, LerpInterval);

        RendererCamera.fieldOfView += CurrSprintFov - PrevSprintFov;

        // Set the previous sprint fov
        PrevSprintFov = CurrSprintFov;
    }

    // Called each render step
    private void Update() {
        // If we don't have network ownership over this object
        // return out of this statement
        if (!isLocalPlayer) { return; }

        UpdateMouseRotation();
        UpdateCameraMotion();

        UpdateSprintingFieldOfView();
    }

    // Called when the object is spawned into the world
    private void Awake() {
        // Make new control inputs
        Inputs = new ControlInputs();
        Inputs.Enable();

        // Get the renderer camera
        RendererCamera = Renderer.GetComponent<Camera>();
    }

    // Called to start freecam
    private void StartFreecam() {
        // Make sure there are no freecams in the scene
        if (Freecam != null) { return; }

        // Summon a Freecam object
        Freecam = Instantiate(FreecamReference);
        Freecam.transform.position = Camera.position;

        // Disable every player camera
        foreach (CameraController Controller in GameObject.FindObjectsOfType(typeof(CameraController))) {
            Controller.Camera.gameObject.SetActive(false);
        }
    }

    // Called to stop freecam
    private void StopFreecam() {
        // Make sure there is a freecam in the scene
        if (Freecam == null) { return; }

        // Destroy the freecam & enable our camera
        Destroy(Freecam);

        // Enable our player camera again
        foreach (CameraController Controller in GameObject.FindObjectsOfType(typeof(CameraController))) {
            if (Controller.isLocalPlayer) {
                Controller.Camera.gameObject.SetActive(true);
            }
        }

        Freecam = null;
    }

    // Called to toggle freecam
    // The spawning of the freecam is buggy! The freecams position
    // will go to the most recently added players position!
    [QFSW.QC.Command]
    private void FreecamEnabled(bool Enabled) {
        if (Enabled) { StartFreecam(); }
        else { StopFreecam(); }
    }

}
