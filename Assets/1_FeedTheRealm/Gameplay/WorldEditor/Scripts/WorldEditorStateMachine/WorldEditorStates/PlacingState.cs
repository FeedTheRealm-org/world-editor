using System.Threading.Tasks;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FTR.Core.Common.Config;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine.WorldEditorStates
{
    public class PlacingState : IWorldEditorState
    {
        private WorldEditorStateMachine worldEditor;
        private LayerMask placementLayerMask;
        private LayerMask worldObjectLayerMask;
        private const float PLACEMENT_OFFSET = 1.0f;

        public PlacingState(WorldEditorStateMachine worldEditor)
        {
            this.worldEditor = worldEditor;
            this.placementLayerMask = worldEditor.Config.PlacementLayerMask;
            this.worldObjectLayerMask = worldEditor.Config.WorldLayerMask;
        }

        public void Enter()
        {
            worldEditor.Log($"Placing mode: {worldEditor.SelectedObject.id}");
        }

        public void Exit() { }

        public void Tick()
        {
            // Preview ghost, raycast cursor, snap grid, etc (later)
        }

        public void OnPrimaryAction()
        {
            _ = OnPrimaryActionAsync();
        }

        private async Task OnPrimaryActionAsync()
        {
            if (
                !Raycaster.TryGetPlacementPoint(
                    worldEditor.playerCamera,
                    worldEditor.inputReader,
                    placementLayerMask | worldObjectLayerMask,
                    out RaycastHit hit
                )
            )
                return;

            PlaceablesLibrary library = worldEditor.placeablesLibrary;

            GameObject instance = await library.GetObject(
                worldEditor.SelectedObject.category,
                worldEditor.SelectedObject.id
            );

            var collider = instance.GetComponentInChildren<Collider>();

            // Check whether we hit a WorldObject layer
            bool hitWorldObject =
                ((1 << hit.collider.gameObject.layer) & worldObjectLayerMask) != 0;

            if (hitWorldObject)
            {
                Vector3 extents = collider.bounds.extents;
                float offset = Vector3.Dot(extents, hit.normal.normalized);
                instance.transform.position = hit.point + hit.normal * offset;
            }
            else
            {
                // Ground placement
                instance.transform.position = hit.point;

                float bottomY = collider.bounds.min.y;
                float desiredBottomY = hit.point.y;

                float correction = desiredBottomY - bottomY - PLACEMENT_OFFSET;

                instance.transform.position += Vector3.up * correction;
            }

            var placeable = instance.GetComponent<IPlaceable>();
            placeable.RegisterPlaceable();
        }

        public void OnSecondaryAction()
        {
            worldEditor.Log("Cancel placement");
            worldEditor.SetState(worldEditor.SelectingState);
        }
    }
}
