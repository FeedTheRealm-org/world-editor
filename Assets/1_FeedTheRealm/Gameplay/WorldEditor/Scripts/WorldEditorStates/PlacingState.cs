using System.Threading.Tasks;
using UnityEngine;

public class PlacingState : IMakerState
{
    private WorldEditorStateMachine worldEditor;

    private LayerMask placementLayerMask = LayerMask.GetMask("Placeable");

    public PlacingState(WorldEditorStateMachine worldEditor)
    {
        this.worldEditor = worldEditor;
    }

    public void Enter()
    {
        worldEditor.Log($"Placing mode: {worldEditor.SelectedObject.DisplayName}");
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
        if (!Raycaster.TryGetPlacementPoint(worldEditor, placementLayerMask, out RaycastHit hit))
        {
            worldEditor.Log("No valid placement point found.", Logging.LogType.Warning);
            return;
        }
        var instance = await worldEditor.SelectedObject.GetPlaceableObject(
            WorldLayers.WorldObjectLayer
        );
        instance.transform.position = hit.point;
        instance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
    }

    public void OnSecondaryAction()
    {
        worldEditor.Log("Cancel placement");
        worldEditor.SetState(new SelectingState(worldEditor));
    }
}
