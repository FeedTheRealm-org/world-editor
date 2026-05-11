using FeedTheRealm.Gameplay.Inputs;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine
{
    public static class Raycaster
    {
        private const float DefaultMaxDistance = 10000f;

        public static bool TryGetPlacementPoint(
            Camera camera,
            InputReader inputReader,
            LayerMask layermask,
            out RaycastHit hit,
            float maxDistance = DefaultMaxDistance
        )
        {
            Ray ray = camera.ScreenPointToRay(inputReader.LastClickPosition);
            return Physics.Raycast(ray, out hit, maxDistance, layermask);
        }

        public static GameObject GetGameObject(
            Camera camera,
            InputReader inputReader,
            LayerMask layermask,
            float maxDistance = DefaultMaxDistance
        )
        {
            if (
                TryGetPlacementPoint(
                    camera,
                    inputReader,
                    layermask,
                    out RaycastHit hit,
                    maxDistance
                )
            )
            {
                return hit.collider.gameObject;
            }
            return null;
        }
    }
}
