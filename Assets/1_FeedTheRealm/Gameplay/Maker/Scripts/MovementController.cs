using UnityEngine;

public class MovementController : MonoBehaviour {
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] GameObject playerObject;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 10f;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector2 inputDirection = Vector2.zero;

    public void Move(Vector2 direction) {
        inputDirection = direction;
    }

    private void Update() {
        float targetSpeed = inputDirection.magnitude > 0 ? moveSpeed : 0;
        float currentSpeed = currentVelocity.magnitude;
        float accelerationRate = inputDirection.magnitude > 0 ? acceleration : deceleration;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationRate * Time.deltaTime);

        if (inputDirection.magnitude > 0) {
            // Get the player's forward and right directions based on where they're looking
            Vector3 forward = playerObject.transform.forward;
            Vector3 right = playerObject.transform.right;

            // Combine input directions relative to where the player is looking
            Vector3 moveDirection = (forward * inputDirection.y + right * inputDirection.x).normalized;
            currentVelocity = moveDirection * currentSpeed;
        } else {
            currentVelocity = Vector3.zero;
        }

        playerObject.transform.position += currentVelocity * Time.deltaTime;
    }
}