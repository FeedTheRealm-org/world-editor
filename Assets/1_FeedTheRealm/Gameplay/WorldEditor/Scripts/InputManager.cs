using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Vector3 lastMousePosition;

    [SerializeField]
    private LayerMask placementLayerMask;

    public event Action OnClicked,
        OnExit;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnClicked?.Invoke();
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnExit?.Invoke();
        }
    }

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = mainCamera.nearClipPlane;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, placementLayerMask))
        {
            lastMousePosition = hitInfo.point;
        }
        return lastMousePosition;
    }
}
