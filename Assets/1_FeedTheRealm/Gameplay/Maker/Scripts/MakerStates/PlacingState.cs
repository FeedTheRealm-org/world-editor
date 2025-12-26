using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public void OnPrimaryAction()
    {
        _ = OnPrimaryActionAsync();
    }

    private async Task OnPrimaryActionAsync()
    {
        Debug.Log("Attempting to place object...");

        if (!TryGetPlacementPoint(out RaycastHit hit, 10000000f))
        {
            Debug.LogWarning("No valid placement point found.");
            return;
        }

        var instance = await maker.SelectedObject.GetWorldObjectInstance();

        instance.transform.position = hit.point;
        instance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        Debug.Log($"Placed object: {maker.SelectedObject.DisplayName}");
    }

    public void OnSecondaryAction()
    {
        Debug.Log("Cancel placement");
        maker.SetState(new SelectingState(maker));
    }

    private bool TryGetPlacementPoint(out RaycastHit hit, float maxDistance = 10000f)
    {
        int placementMask = LayerMask.GetMask("Placement");
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = maker.playerCamera.ScreenPointToRay(mousePos);
        return Physics.Raycast(ray, out hit, maxDistance, placementMask);
    }
}
