using FeedTheRealm.Gameplay.Inputs;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Editor Control Configuration")]
        [SerializeField]
        private MovementController movementController;

        [SerializeField]
        private CameraController cameraController;

        [Header("General settings")]
        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private InputReader inputReader;

        [Inject]
        public void Construct(InputReader inputReader)
        {
            this.inputReader = inputReader;
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.MoveEvent += OnMoveInput;
                inputReader.LookEvent += OnLookInput;
                inputReader.MoveVerticalEvent += OnMoveVertical;
                logger.Log("PlayerController subscribed to events.", this);
            }
        }

        private void OnDisable()
        {
            // Unregister callbacks
            if (inputReader != null)
            {
                inputReader.MoveEvent -= OnMoveInput;
                inputReader.LookEvent -= OnLookInput;
                inputReader.MoveVerticalEvent -= OnMoveVertical;
                logger.Log("PlayerController unsubscribed from events.", this);
            }
        }

        private void OnMoveInput(Vector2 vec)
        {
            movementController.Move(vec);
        }

        private void OnLookInput(Vector2 vec)
        {
            cameraController.Look(vec);
        }

        private void OnMoveVertical(float value)
        {
            movementController.MoveVertical(value);
        }
    }
}
