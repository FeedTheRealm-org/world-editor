using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine.WorldEditorStates
{
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
            var WorldObject = selectedObject.GetComponentInParent<IPlaceable>();
            var placeableCategory = WorldObject.Category;
            worldEditor.editPlaceableEvent.Raise(
                new EditableOption { placeable = selectedObject, category = placeableCategory }
            );
        }

        public void OnSecondaryAction() { }
    }
}
