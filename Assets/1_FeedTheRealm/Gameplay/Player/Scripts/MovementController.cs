using FeedTheRealm.Core.EventChannels.Ticks;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.Player
{
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

        public void Move(Vector2 direction) => inputDirection = direction;

        public void MoveVertical(float direction) => verticalInput = direction;

        private void OnEnable() => tickEvent.OnRaised += Tick;

        private void OnDisable() => tickEvent.OnRaised -= Tick;

        private void Tick()
        {
            UpdateVelocity();
            ApplyMovement();
        }

        private void UpdateVelocity()
        {
            bool isMoving = inputDirection.magnitude > 0;
            float targetSpeed = isMoving ? config.moveSpeed : 0;
            float accelerationRate = isMoving ? config.acceleration : config.deceleration;
            float currentSpeed = Mathf.Lerp(
                currentVelocity.magnitude,
                targetSpeed,
                accelerationRate * Time.deltaTime
            );

            currentVelocity = isMoving ? CalculateMoveDirection() * currentSpeed : Vector3.zero;
        }

        private Vector3 CalculateMoveDirection()
        {
            Vector3 forward = playerObject.transform.forward;
            Vector3 right = playerObject.transform.right;
            return (forward * inputDirection.y + right * inputDirection.x).normalized;
        }

        private void ApplyMovement()
        {
            playerObject.transform.position += currentVelocity * Time.deltaTime;
            playerObject.transform.position +=
                verticalInput * config.verticalSpeed * Time.deltaTime * Vector3.up;
        }
    }
}
