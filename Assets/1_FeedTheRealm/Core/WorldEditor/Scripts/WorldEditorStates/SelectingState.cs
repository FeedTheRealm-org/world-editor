using UnityEngine;

public class SelectingState : IMakerState
{
    private readonly WorldEditorStateMachine maker;

    public SelectingState(WorldEditorStateMachine maker)
    {
        this.maker = maker;
    }

    public void Enter()
    {
        maker.Log("Selecting objects");
    }

    public void Exit() { }

    public void Tick() { }

    public void OnPrimaryAction()
    {
        GameObject selectedObject = Raycaster.GetGameObject(
            maker,
            WorldLayers.WorldObjectLayerMask
        );
        if (!selectedObject)
        {
            return;
        }
        if (selectedObject.TryGetComponent<ISelectable>(out var selectable))
            selectable.OnObjectSelected();
        else
            maker.Log("The selected object is not selectable", Logging.LogType.Warning);
    }

    public void OnSecondaryAction() { }
}
