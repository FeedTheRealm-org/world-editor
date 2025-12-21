using UnityEngine;

public class MovementController : MonoBehaviour {
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Transform playerObject;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 10f;
    [SerializeField] float vertialSpeed = 3f;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector2 inputDirection = Vector2.zero;
    private float verticalInput = 0f;

    public void Move(Vector2 direction) {
        inputDirection = direction;
    }

    public void MoveVertical(float direction) {
        verticalInput = direction;
    }

    private void Update() {
        float targetSpeed = inputDirection.magnitude > 0 ? moveSpeed : 0;
        float currentSpeed = currentVelocity.magnitude;
        float accelerationRate = inputDirection.magnitude > 0 ? acceleration : deceleration;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationRate * Time.deltaTime);

        if (inputDirection.magnitude > 0) {
            Vector3 forward = playerObject.transform.forward;
            Vector3 right = playerObject.transform.right;
            Vector3 moveDirection = (forward * inputDirection.y + right * inputDirection.x).normalized;
            currentVelocity = moveDirection * currentSpeed;
        } else {
            currentVelocity = Vector3.zero;
        }

        playerObject.transform.position += currentVelocity * Time.deltaTime;
        playerObject.transform.position += verticalInput * vertialSpeed * Time.deltaTime * Vector3.up;
    }
}