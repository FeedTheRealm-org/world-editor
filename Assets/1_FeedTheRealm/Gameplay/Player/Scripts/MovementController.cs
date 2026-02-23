using FeedTheRealm.Core.EventChannels.Ticks;
using UnityEngine;
using VContainer;

public class MovementController : MonoBehaviour
{
    [SerializeField]
    private PlayerConfig config;

    [SerializeField]
    private Transform playerObject;

    [Inject]
    private TickEvent tickEvent;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector2 inputDirection = Vector2.zero;
    private float verticalInput = 0f;

    public void Move(Vector2 direction)
    {
        inputDirection = direction;
    }

    public void MoveVertical(float direction)
    {
        verticalInput = direction;
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
        float targetSpeed = inputDirection.magnitude > 0 ? config.moveSpeed : 0;
        float currentSpeed = currentVelocity.magnitude;
        float accelerationRate =
            inputDirection.magnitude > 0 ? config.acceleration : config.deceleration;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationRate * Time.deltaTime);

        if (inputDirection.magnitude > 0)
        {
            Vector3 forward = playerObject.transform.forward;
            Vector3 right = playerObject.transform.right;
            Vector3 moveDirection = (
                forward * inputDirection.y + right * inputDirection.x
            ).normalized;
            currentVelocity = moveDirection * currentSpeed;
        }
        else
        {
            currentVelocity = Vector3.zero;
        }

        playerObject.transform.position += currentVelocity * Time.deltaTime;
        playerObject.transform.position +=
            verticalInput * config.verticalSpeed * Time.deltaTime * Vector3.up;
    }
}
