using UnityEngine;

public class RemovingState : IMakerState
{
    private MakerStateMachine maker;

    public RemovingState(MakerStateMachine maker)
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
        Debug.Log("Removing object under cursor");
    }

    public void OnSecondaryAction()
    {
        maker.SetState(new SelectingState(maker));
    }
}
