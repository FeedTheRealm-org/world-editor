using FeedTheRealm.Core.Interfaces;
using UnityEngine;

public class SelectingState : IWorldEditorState
{
    private readonly WorldEditorStateMachine worldEditor;

    public SelectingState(WorldEditorStateMachine worldEditor)
    {
        this.worldEditor = worldEditor;
    }

    public void Enter()
    {
        worldEditor.Log("Selecting objects");
    }

    public void Exit() { }

    public void Tick() { }

    public void OnPrimaryAction()
    {
        GameObject selectedObject = Raycaster.GetGameObject(
            worldEditor,
            WorldLayers.WorldObjectLayerMask
        );
        if (!selectedObject)
            return;
        worldEditor.SetState(new EditingState(worldEditor, selectedObject));
    }

    public void OnSecondaryAction() { }
}
