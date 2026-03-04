using System;
using FeedTheRealm.Core.Interfaces;
using UnityEngine;

public class EditingState : IWorldEditorState
{
    private readonly WorldEditorStateMachine worldEditor;

    public Action CloseEditorEventCallback;

    public IEditable selectableComponent;

    public EditingState(WorldEditorStateMachine worldEditor, GameObject selectedObject)
    {
        this.worldEditor = worldEditor;

        CloseEditorEventCallback = () =>
        {
            worldEditor.Log("Edit completed, returning to selection mode");
            worldEditor.SetState(worldEditor.SelectingState);
        };

        if (selectedObject.TryGetComponent<IEditable>(out var selectable))
            selectableComponent = selectable;
        else
            worldEditor.Log("The selected object is not selectable", Logging.LogType.Warning);
    }

    public void Enter()
    {
        selectableComponent?.OnObjectSelected(CloseEditorEventCallback);
    }

    public void Exit()
    {
        selectableComponent?.OnObjectDeselected();
    }

    public void Tick() { }

    public void OnPrimaryAction() { }

    public void OnSecondaryAction()
    {
        worldEditor.Log("Cancel edit mode");
        worldEditor.SetState(worldEditor.SelectingState);
    }
}
