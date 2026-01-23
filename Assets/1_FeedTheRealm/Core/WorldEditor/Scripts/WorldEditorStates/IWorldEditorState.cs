public interface IWorldEditorState
{
    void Enter();
    void Exit();
    void Tick(); // used for continuous updates (hovering, preview ghosts, raycasts)
    void OnPrimaryAction(); // left click
    void OnSecondaryAction(); // right click
}
