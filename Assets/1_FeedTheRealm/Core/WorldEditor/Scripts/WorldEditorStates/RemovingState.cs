using UnityEngine;

public class RemovingState : IWorldEditorState
{
    private WorldEditorStateMachine worldEditor;

    private LayerMask objectLayerMask = LayerMask.GetMask("WorldObject");

    public RemovingState(WorldEditorStateMachine worldEditor)
    {
        this.worldEditor = worldEditor;
    }

    public void Enter()
    {
        Debug.Log("ENTER: Removing Mode");
    }

    public void Exit()
    {
        Debug.Log("EXIT: Removing Mode");
    }

    public void Tick() { }

    public void OnPrimaryAction()
    {
        worldEditor.Log($"ObjectLayerMask value: {objectLayerMask.value}");
        if (!Raycaster.TryGetPlacementPoint(worldEditor, objectLayerMask, out RaycastHit hit))
        {
            worldEditor.Log("No objects to remove.");
            worldEditor.Log($"Hit object: {hit.collider}");
            return;
        }
        GameObject hitObject = hit.collider.gameObject;
        worldEditor.Log($"Removing object: {hitObject.name}");
        GameObject rootObject = hitObject;
        while (rootObject.transform.parent != null)
        {
            rootObject = rootObject.transform.parent.gameObject;
        }

        worldEditor.Log($"Removing object: {rootObject.name}");
        Object.Destroy(rootObject);
    }

    public void OnSecondaryAction()
    {
        worldEditor.Log("Cancel removing mode");
        worldEditor.SetState(worldEditor.SelectingState);
    }
}
