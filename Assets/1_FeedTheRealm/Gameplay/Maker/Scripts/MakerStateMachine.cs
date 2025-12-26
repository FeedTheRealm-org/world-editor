using UnityEngine;

public class MakerStateMachine : MonoBehaviour
{
    [SerializeField]
    public Camera playerCamera;
    private IMakerState currentState;

    public WorldObjectReference SelectedObject { get; private set; }

    void Start()
    {
        SetState(new SelectingState(this));
    }

    private void OnEnable()
    {
        Utils.WorldObjectSelectionEvents.ObjectSelected += OnWorldObjectSelected;
    }

    private void OnDisable()
    {
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

    public void HandlePrimaryInteraction()
    {
        currentState?.OnPrimaryAction();
    }

    public void HandleSecondaryInteraction()
    {
        currentState?.OnSecondaryAction();
    }

    private void Update()
    {
        currentState?.Tick();
    }
}
