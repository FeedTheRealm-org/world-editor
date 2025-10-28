using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour {

    [Header("Move Params")]
    public float maxSpeed = 30f;
    public float acceleration = 100f;

    public Vector3 CurrentVelocity { get; private set; }
    public float currentSpeed { get; private set; }

    [Header("Looking")]
    public Vector2 lookSensitivity = new Vector2(1f, 1f);
    public float pitchLimit = 85f;
    [SerializeField] float currentPitch = 0f;

    [Header("Inputs")]
    public Vector3 MoveInput;
    public Vector2 LookInput;

    public float CurrentPitch {
        get { return currentPitch; }
        set {
            currentPitch = Mathf.Clamp(value, -pitchLimit, pitchLimit);
        }
    }


    [Header("Components")]
    [SerializeField] CinemachineCamera fpCam;
    [SerializeField] CharacterController controller;

    #region Unity Methods
    private void OnValidate() {
        if (controller == null)
            controller = GetComponent<CharacterController>();

    }

    private void Update() {
        MoveUpdate();
        LookUpdate();
    }

    #region Controller Methods

    void MoveUpdate() {
        // Get the forward and right directions from the camera, not from the player body
        Vector3 camForward = fpCam.transform.forward;
        Vector3 camRight = fpCam.transform.right;
        Vector3 camUp = fpCam.transform.up; // now we include up/down motion

        // We build motion relative to where the camera is looking
        Vector3 motion = camForward * MoveInput.y + camRight * MoveInput.x + camUp * MoveInput.z;

        // Normalize to avoid faster diagonal movement
        if (motion.sqrMagnitude > 1f)
            motion.Normalize();

        // Apply acceleration and max speed
        if (motion.sqrMagnitude >= 0.01f) {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * maxSpeed, acceleration * Time.deltaTime);
        } else {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, acceleration * Time.deltaTime);
        }

        // Move the controller
        controller.Move(CurrentVelocity * Time.deltaTime);

        currentSpeed = CurrentVelocity.magnitude;
    }

    void LookUpdate() {
        Vector2 input = new Vector2(
            LookInput.x * lookSensitivity.x,
            LookInput.y * lookSensitivity.y
        );
        // This is for looking up and down
        CurrentPitch -= input.y;
        fpCam.transform.localEulerAngles = new Vector3(CurrentPitch, 0f, 0f);
        // This is for looking left and right
        transform.Rotate(Vector3.up * input.x);
    }

    #endregion


    #endregion


}
