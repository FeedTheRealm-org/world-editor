using UnityEngine;

public class WorldEditorStateMachine : MonoBehaviour
{
    [SerializeField]
    public MakerInputReader inputReader;

    [SerializeField]
    public Camera playerCamera;

    [SerializeField]
    private Logging.Logger logger;
    private IWorldEditorState currentState;
    public IPlaceable SelectedObject { get; private set; }

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
        inputReader.PrimaryInteractionEvent += OnPrimaryInteraction;
        inputReader.SecondaryInteractionEvent += OnSecondaryInteraction;
        Utils.SelectionRaiser.ObjectSelected -= OnWorldObjectSelected;
    }

    private void OnWorldObjectSelected(IPlaceable reference)
    {
        SelectedObject = reference;
        SetState(new PlacingState(this));
    }

    public void SetState(IWorldEditorState newState)
    {
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
