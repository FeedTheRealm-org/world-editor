using UnityEngine;

public class WorldEditorStateMachine : MonoBehaviour
{
    [SerializeField]
    private Logging.Logger logger;

    [Header("References | DO NOT SET IN INSPECTOR")]
    public MakerInputReader inputReader;
    public Camera playerCamera;
    public IPlaceable SelectedObject { get; private set; }
    public bool EnableEditor = true;

    public void ToggleEditor(bool status)
    {
        if (!status)
            currentState?.Exit();
        EnableEditor = status;
    }

    private IWorldEditorState currentState;

    void Start()
    {
        SetState(new SelectingState(this));
    }

    private void OnEnable()
    {
        inputReader.PrimaryInteractionEvent += OnPrimaryInteraction;
        inputReader.SecondaryInteractionEvent += OnSecondaryInteraction;
        inputReader.RemoveEvent += OnRemoveAction;
        Utils.SelectionRaiser.ObjectSelected += OnWorldObjectSelected;
    }

    private void OnDisable()
    {
        inputReader.PrimaryInteractionEvent -= OnPrimaryInteraction;
        inputReader.SecondaryInteractionEvent -= OnSecondaryInteraction;
        Utils.SelectionRaiser.ObjectSelected -= OnWorldObjectSelected;
    }

    private void OnWorldObjectSelected(IPlaceable reference)
    {
        if (currentState is PlacingState)
        {
            SelectedObject = reference;
            Log($"Switched to: {reference.DisplayName}");
            return;
        }
        SelectedObject = reference;
        SetState(new PlacingState(this));
    }

    public void SetState(IWorldEditorState newState)
    {
        if (!EnableEditor)
            return;

        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void OnPrimaryInteraction()
    {
        currentState?.OnPrimaryAction();
    }

    public void OnSecondaryInteraction()
    {
        currentState?.OnSecondaryAction();
    }

    public void OnRemoveAction()
    {
        SetState(new RemovingState(this));
    }

    public void Log(string message, Logging.LogType type = Logging.LogType.Info)
    {
        logger.Log(message, this, type);
    }

    private void Update()
    {
        currentState?.Tick();
    }
}
