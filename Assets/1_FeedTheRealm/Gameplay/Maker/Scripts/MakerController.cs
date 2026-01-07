using UnityEngine;

public class MakerController : MonoBehaviour
{
    [Header("Editor Control Configuration")]
    [SerializeField]
    public MovementController movementController;

    [SerializeField]
    public CameraController cameraController;

    [Header("HUD Configuration")]
    [SerializeField]
    public SettingsMenuController settingsMenu;

    [Header("General settings")]
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    public MakerInputReader inputReader;

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.MoveEvent += OnMoveInput;
            inputReader.LookEvent += OnLookInput;
            inputReader.MoveVerticalEvent += OnMoveVertical;
            inputReader.CursorToggleEvent += OnToggleSettingsMenu;
            logger.Log("MakerController subscribed from events.", this);
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
            inputReader.CursorToggleEvent -= OnToggleSettingsMenu;
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

    public void OnToggleSettingsMenu()
    {
        settingsMenu.ToggleSettings();
    }
}
