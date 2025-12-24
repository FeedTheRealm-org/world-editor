using UnityEngine;

public class SelectingState : IMakerState
{
    private readonly MakerStateMachine maker;

    public SelectingState(MakerStateMachine maker)
    {
        this.maker = maker;
    }

    public void Enter()
    {
        Debug.Log("Selecting objects");
    }

    public void Exit() { }

    public void Tick() { }

    public void OnPrimaryAction()
    {
        Debug.Log("Select object in world");
    }

    public void OnSecondaryAction() { }
}
