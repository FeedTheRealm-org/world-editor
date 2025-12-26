using UnityEngine;

public class WorldEditorStateMachine : MonoBehaviour
{
    [SerializeField]
    public MakerInputReader inputReader;

    [SerializeField]
    public Camera playerCamera;

    [SerializeField]
    private Logging.Logger logger;
    private IMakerState currentState;

    public WorldObjectReference SelectedObject { get; private set; }

    void Start()
    {
        SetState(new SelectingState(this));
    }

    private void OnEnable()
    {
        inputReader.PrimaryInteractionEvent += OnPrimaryInteraction;
        inputReader.SecondaryInteractionEvent += OnSecondaryInteraction;
        Utils.WorldObjectSelectionEvents.ObjectSelected += OnWorldObjectSelected;
    }

    private void OnDisable()
    {
        inputReader.PrimaryInteractionEvent += OnPrimaryInteraction;
        inputReader.SecondaryInteractionEvent += OnSecondaryInteraction;
        Utils.WorldObjectSelectionEvents.ObjectSelected -= OnWorldObjectSelected;
    }

    private void OnWorldObjectSelected(WorldObjectReference reference)
    {
        SelectedObject = reference;
        SetState(new PlacingState(this));
    }

    public void SetState(IMakerState newState)
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

    private void Update()
    {
        currentState?.Tick();
    }
}
