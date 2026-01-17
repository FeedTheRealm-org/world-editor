using System.Threading.Tasks;
using UnityEngine;

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
        try
        {
            if (
                !Raycaster.TryGetPlacementPoint(worldEditor, placementLayerMask, out RaycastHit hit)
            )
            {
                worldEditor.Log("No valid placement point found.", Logging.LogType.Warning);
                Debug.LogWarning("No valid placement point found");
                return;
            }

            Debug.Log($"Hit point: {hit.point}, Normal: {hit.normal}");

            var instance = await worldEditor.SelectedObject.GetPlaceableObject(
                WorldLayers.WorldObjectLayer
            );
            if (instance == null)
            {
                worldEditor.Log("Failed to get placeable object instance.", Logging.LogType.Error);
                Debug.LogError("Failed to get placeable object instance");
                return;
            }
            instance.transform.position = hit.point;
            instance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            Debug.Log($"Placed object at {hit.point} with rotation {instance.transform.rotation}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in OnPrimaryActionAsync: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void OnSecondaryAction()
    {
        worldEditor.Log("Cancel placement");
        worldEditor.SetState(new SelectingState(worldEditor));
    }
}
