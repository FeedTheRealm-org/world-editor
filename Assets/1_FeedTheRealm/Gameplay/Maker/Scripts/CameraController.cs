using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private Transform playerObject;
    [SerializeField] private Vector2 lookSensitivity = new(1f, 1f);
    [SerializeField] private float pitchLimit = 85f;
    [SerializeField] float currentPitch = 0f;

    private Vector2 lookInput = Vector2.zero;

    public void Look(Vector2 input) {
        lookInput = input;
    }

    private void Update() {
        if (lookInput.magnitude == 0) return;

        // Vertical rotation (pitch) - clamped
        currentPitch -= lookInput.y * lookSensitivity.y;
        currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);
        playerObject.transform.localRotation = Quaternion.Euler(currentPitch, playerObject.transform.localEulerAngles.y, 0f);

        // Horizontal rotation (yaw)
        float yaw = playerObject.transform.localEulerAngles.y;
        yaw += lookInput.x * lookSensitivity.x;
        playerObject.transform.localRotation = Quaternion.Euler(currentPitch, yaw, 0f);
    }
}
