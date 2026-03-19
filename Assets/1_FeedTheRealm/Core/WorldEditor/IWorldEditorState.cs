namespace FeedTheRealm.Core.WorldEditor
{
    public interface IWorldEditorState
    {
        void Enter();
        void Exit();
        void OnPrimaryAction(); // left click
        void OnSecondaryAction(); // right click
    }
}
