using UnityEngine;

public class SelectingState : MonoBehaviour, IMakerState
{
    private readonly WorldEditorStateMachine maker;

    public SelectingState(WorldEditorStateMachine maker)
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
