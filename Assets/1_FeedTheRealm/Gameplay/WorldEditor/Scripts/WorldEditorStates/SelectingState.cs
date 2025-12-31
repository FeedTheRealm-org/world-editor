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
        maker.Log("Select object in world");
    }

    public void OnSecondaryAction() { }
}
