using UnityEngine;
using QFSW.QC;

public class Freecam : MonoBehaviour {

    [SerializeField] private float DEFAULT_MOUSE_SENSITIVITY = 0.35f;
    [SerializeField] private float DEFAULT_CAMERA_SPEED = 10.0f;

    private float CameraSpeed = 1.0f;
    private float Acceleration = -1.0f;
    private float LookAcceleration = -1.0f;

    private Vector2 CurrentLookDirection;
    private Vector3 CurrentMoveDirection, CurrentVerticalDirection;

    private float SensitivityMultiplier = 1.0f;

    private Camera CameraComp;

    // Called to set the freecams movementspeed
    [Command]
    private void SetFreecamMovementSpeed(float Speed) {
        CameraSpeed = Speed;
    }

    // Called to set the freecams acceleration
    [Command]
    private void SetFreecamMoveAcceleration(float AccelValue) {
        Acceleration = AccelValue;
    }

    // Called to set the freecams looking acceleration
    [Command]
    private void SetFreecamLookAcceleration(float AccelValue) {
        LookAcceleration = AccelValue;
    }

    // Called to se the freecams field of view
    [Command]
    private void SetFreecamFieldOfView(float FieldOfView) {
        CameraComp.fieldOfView = FieldOfView;
    }

    // Called to get the user inputed move vector
    private Vector2 GetRawMoveVector() {
        Vector2 Result = new Vector2();
        Result.x += Input.GetAxisRaw("Horizontal");
        Result.y += Input.GetAxisRaw("Vertical");

        return Result;
    }

    // Called to get the move direction
    private Vector3 GetMoveDirection() {
        Vector2 RawMoveVector = GetRawMoveVector();
        Vector3 MoveDirection = (transform.forward * RawMoveVector.y) + (transform.right * RawMoveVector.x);

        return MoveDirection.normalized;
    }

    // Called to update the WASD movement
    private void DirectionalMovement() {
        Vector3 MoveDirection = GetMoveDirection() * (DEFAULT_CAMERA_SPEED * CameraSpeed);

        // If there is no acceleration then just increase by the move direction
        if (Acceleration <= 0.0f || Acceleration >= 100.0f) {
            CurrentMoveDirection = MoveDirection;
        }
        else {
            CurrentMoveDirection = Vector3.Lerp(CurrentMoveDirection, MoveDirection, Acceleration * Time.deltaTime);
        }

        // Slow down if we're like holding shift bro
        float SpeedShift = Input.GetKey(KeyCode.LeftShift) ? 0.5f : 1.0f;
        
        // Move the thingy
        transform.position += CurrentMoveDirection * Time.deltaTime * SpeedShift;
    }

    // Called to update the mouse movement
    private void MouseMovement() {
        float Sens = DEFAULT_MOUSE_SENSITIVITY * SensitivityMultiplier;

        float MouseX = Input.GetAxisRaw("Mouse X") * Sens;
        float MouseY = -Input.GetAxisRaw("Mouse Y") * Sens;

        Vector2 NextLookDirection = new Vector2(Mathf.Clamp(CurrentLookDirection.x + MouseY, -90.0f, 90.0f), CurrentLookDirection.y + MouseX);
        if (LookAcceleration <= 0.0f || LookAcceleration >= 100.0f) {
            CurrentLookDirection = NextLookDirection;
        }
        else {
            CurrentLookDirection = Vector2.Lerp(CurrentLookDirection, NextLookDirection, Mathf.Min(LookAcceleration * Time.deltaTime, 1.0f));
        }

        transform.localRotation = Quaternion.Euler(CurrentLookDirection);
    }

    // Called each renderstep
    private void Update() {
        DirectionalMovement();
        MouseMovement();
    }

    // Called when this object is spawned into the world
    private void Awake() {
        CameraComp = GetComponent<Camera>();
    }

}
