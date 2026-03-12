using FeedTheRealm.Core.EventChannels.Ticks;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.Player
{
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

            currentPitch = CalculatePitch();
            float yaw = CalculateYaw();

            playerObject.transform.localRotation = Quaternion.Euler(currentPitch, yaw, 0f);
        }

        private float CalculatePitch()
        {
            float newPitch = currentPitch - lookInput.y * config.lookSensitivity.y;
            return Mathf.Clamp(newPitch, -config.pitchLimit, config.pitchLimit);
        }

        private float CalculateYaw()
        {
            return playerObject.transform.localEulerAngles.y
                + lookInput.x * config.lookSensitivity.x;
        }
    }
}
