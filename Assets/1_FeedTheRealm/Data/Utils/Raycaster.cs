using UnityEngine;
using UnityEngine.InputSystem;

public static class Raycaster
{
    private const float DefaultMaxDistance = 10000f;

    public static bool TryGetPlacementPoint(
        WorldEditorStateMachine maker,
        LayerMask layermask,
        out RaycastHit hit,
        float maxDistance = DefaultMaxDistance
    )
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = maker.playerCamera.ScreenPointToRay(mousePos);
        return Physics.Raycast(ray, out hit, maxDistance, layermask);
    }
}
