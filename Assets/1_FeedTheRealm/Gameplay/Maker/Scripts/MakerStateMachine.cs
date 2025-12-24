using UnityEngine;

public class MakerStateMachine : MonoBehaviour
{
    [SerializeField]
    private MakerInputReader inputReader;

    private IMakerState currentState;

    public WorldObjectReference SelectedObject { get; private set; }

    void Start()
    {
        SetState(new SelectingState(this));
    }

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.PrimaryInteractionEvent += OnPrimaryInteraction;
            inputReader.SecondaryInteractionEvent += OnSecondaryInteraction;
        }

        WorldObjectSelectionEvents.ObjectSelected += OnWorldObjectSelected;
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.PrimaryInteractionEvent -= OnPrimaryInteraction;
            inputReader.SecondaryInteractionEvent -= OnSecondaryInteraction;
        }

        WorldObjectSelectionEvents.ObjectSelected -= OnWorldObjectSelected;
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

    private void OnPrimaryInteraction()
    {
        currentState?.OnPrimaryAction();
    }

    private void OnSecondaryInteraction()
    {
        currentState?.OnSecondaryAction();
    }

    private void Update()
    {
        currentState?.Tick();
    }
}
