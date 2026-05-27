using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine.WorldEditorStates
{
    public class RemovingState : IWorldEditorState
    {
        private WorldEditorStateMachine worldEditor;

        private LayerMask objectLayerMask = LayerMask.GetMask("WorldObject");

        public RemovingState(WorldEditorStateMachine worldEditor)
        {
            this.worldEditor = worldEditor;
        }

        public void Enter()
        {
            worldEditor.editorStateChangedEvent.Raise(EditorStates.Removing);
            worldEditor.Log("Removing mode");
        }

        public void Exit()
        {
            worldEditor.editorStateChangedEvent.Raise(EditorStates.None);
        }

        public void Tick() { }

        public void OnPrimaryAction()
        {
            if (
                !Raycaster.TryGetPlacementPoint(
                    worldEditor.playerCamera,
                    worldEditor.inputReader,
                    objectLayerMask,
                    out RaycastHit hit
                )
            )
            {
                worldEditor.Log("No objects to remove.");
                return;
            }
            GameObject selectedPlaceable = hit.collider.gameObject;
            var WorldObject = selectedPlaceable.GetComponentInParent<IPlaceable>();
            WorldObject.DeletePlaceable();
        }

        public void OnSecondaryAction()
        {
            worldEditor.SetState(worldEditor.SelectingState);
        }
    }
}
