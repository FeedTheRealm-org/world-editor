using UnityEngine;

public class RemovingState : MonoBehaviour, IMakerState
{
    private WorldEditorStateMachine maker;

    private readonly LayerMask placementLayerMask = LayerMask.GetMask("Removal");

    public RemovingState(WorldEditorStateMachine maker)
    {
        this.maker = maker;
    }

    public void Enter()
    {
        Debug.Log("ENTER: Removing Mode");
    }

    public void Exit() { }

    public void Tick() { }

    public void OnPrimaryAction()
    {
        // if (!Raycaster.TryGetPlacementPoint(maker, placementLayerMask, out RaycastHit hit))
        // {
        //     Debug.LogWarning("No valid placement point found.");
        //     return;
        // }
        // var instance = await maker.SelectedObject.GetWorldObjectInstance();
        // instance.transform.position = hit.point;
        // instance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
    }

    public void OnSecondaryAction()
    {
        maker.SetState(new SelectingState(maker));
    }
}
