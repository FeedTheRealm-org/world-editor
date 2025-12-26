using System.Threading.Tasks;
using UnityEngine;

public class PlacingState : MonoBehaviour, IMakerState
{
    private readonly WorldEditorStateMachine maker;

    private readonly LayerMask placementLayerMask = LayerMask.GetMask("Placeable");

    public PlacingState(WorldEditorStateMachine maker)
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
        if (!Raycaster.TryGetPlacementPoint(maker, placementLayerMask, out RaycastHit hit))
        {
            Debug.LogWarning("No valid placement point found.");
            return;
        }
        var instance = await maker.SelectedObject.GetWorldObjectInstance();
        instance.transform.position = hit.point;
        instance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
    }

    public void OnSecondaryAction()
    {
        Debug.Log("Cancel placement");
        maker.SetState(new SelectingState(maker));
    }
}
