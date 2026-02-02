using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class PlacingState : IWorldEditorState
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
            return;

        GameObject instance = await worldEditor.SelectedObject.GetPlaceableObject(
            WorldLayers.WorldObjectLayer
        );
        instance.transform.position = hit.point;
        var collider = instance.GetComponentInChildren<Collider>();
        float bottomY = collider.bounds.min.y;
        float desiredBottomY = hit.point.y;
        float correction = desiredBottomY - bottomY - 1;
        instance.transform.position += Vector3.up * correction;
    }

    public void OnSecondaryAction()
    {
        worldEditor.Log("Cancel placement");
        worldEditor.SetState(worldEditor.SelectingState);
    }
}
