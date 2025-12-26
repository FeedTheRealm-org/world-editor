using UnityEngine;

public class MakerController : MonoBehaviour
{
    [SerializeField]
    public MakerInputReader inputReader;

    [SerializeField]
    public MovementController movementController;

    [SerializeField]
    public CameraController cameraController;

    [SerializeField]
    public MakerStateMachine stateMachine;

    [SerializeField]
    private Logging.Logger logger;

    private void OnEnable()
    {
        // Register callbacks
        if (inputReader != null)
        {
            inputReader.MoveEvent += OnMoveInput;
            inputReader.LookEvent += OnLookInput;
            inputReader.MoveVerticalEvent += OnMoveVertical;
            logger.Log("MakerController subscribed from events.", this);
            inputReader.PrimaryInteractionEvent += OnPrimaryInteraction;
            inputReader.SecondaryInteractionEvent += OnSecondaryInteraction;
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
            inputReader.PrimaryInteractionEvent -= OnPrimaryInteraction;
            inputReader.SecondaryInteractionEvent -= OnSecondaryInteraction;
            logger.Log("MakerController unsubscribed from events.", this);
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

    private void OnPrimaryInteraction()
    {
        stateMachine.HandlePrimaryInteraction();
    }

    private void OnSecondaryInteraction()
    {
        stateMachine.HandleSecondaryInteraction();
    }
}
