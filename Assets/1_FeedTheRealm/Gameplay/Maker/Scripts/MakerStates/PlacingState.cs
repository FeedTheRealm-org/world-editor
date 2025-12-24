using UnityEngine;

public class PlacingState : IMakerState
{
    private readonly MakerStateMachine maker;

    public PlacingState(MakerStateMachine maker)
    {
        this.maker = maker;
    }

    public void Enter()
    {
        Debug.Log($"Placing mode: {maker.SelectedObject.DisplayName}");
    }

    public void Exit() { }

    public void Tick()
    {
        // Preview ghost, raycast cursor, snap grid, etc (later)
    }

    public async void OnPrimaryAction()
    {
        Debug.Log("Place object");

        var instance = await maker.SelectedObject.GetWorldObjectInstance();
        instance.transform.position = GetPlacementPosition();

        maker.SetState(new SelectingState(maker));
    }

    public void OnSecondaryAction()
    {
        Debug.Log("Cancel placement");
        maker.SetState(new SelectingState(maker));
    }

    private Vector3 GetPlacementPosition()
    {
        // Temporary placeholder
        return Vector3.zero;
    }
}
