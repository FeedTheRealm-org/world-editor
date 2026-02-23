using FeedTheRealm.Core.EventChannels.Ticks;
using UnityEngine;
using VContainer;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform playerObject;

    [SerializeField]
    private PlayerConfig config;

    [SerializeField]
    float currentPitch = 0f;

    [Inject]
    private TickEvent tickEvent;

    private Vector2 lookInput = Vector2.zero;

    public void Look(Vector2 input)
    {
        lookInput = input;
    }

    private void OnEnable()
    {
        tickEvent.OnRaised += Tick;
    }

    private void OnDisable()
    {
        tickEvent.OnRaised -= Tick;
    }

    private void Tick()
    {
        if (lookInput.magnitude == 0)
            return;

        // Vertical rotation (pitch) - clamped
        currentPitch -= lookInput.y * config.lookSensitivity.y;
        currentPitch = Mathf.Clamp(currentPitch, -config.pitchLimit, config.pitchLimit);
        playerObject.transform.localRotation = Quaternion.Euler(
            currentPitch,
            playerObject.transform.localEulerAngles.y,
            0f
        );

        // Horizontal rotation (yaw)
        float yaw = playerObject.transform.localEulerAngles.y;
        yaw += lookInput.x * config.lookSensitivity.x;
        playerObject.transform.localRotation = Quaternion.Euler(currentPitch, yaw, 0f);
    }
}
