using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DragAndDrop : MonoBehaviour {
    private Camera mainCamera;
    private bool isDragging = false;
    private float distanceFromCamera;
    private float targetDistance;
    private Vector3 offset;

    [Header("Zoom Settings")]
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private float zoomSmoothness = 10f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 200f;

    private Plane groundPlane;

    void Start() {
        mainCamera = Camera.main;
        groundPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
    }

    void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryStartDragging();

        if (isDragging && Mouse.current.leftButton.isPressed) {
            HandleScroll();
            UpdateDrag();
        }

        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            StopDragging();
    }

    private void TryStartDragging() {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.transform == transform) {
                isDragging = true;
                distanceFromCamera = Vector3.Distance(mainCamera.transform.position, hit.point);
                targetDistance = distanceFromCamera;
                offset = transform.position - hit.point;
            }
        }
    }

    private void HandleScroll() {
        float scrollDelta = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scrollDelta) > 0.01f) {
            targetDistance -= scrollDelta * scrollSpeed * Time.deltaTime;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        distanceFromCamera = Mathf.Lerp(distanceFromCamera, targetDistance, Time.deltaTime * zoomSmoothness);
    }

    private void UpdateDrag() {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 targetPoint;
        targetPoint = ray.GetPoint(distanceFromCamera);
        Vector3 desiredPosition = targetPoint + offset;
        transform.position = desiredPosition;
    }

    private void StopDragging() {
        isDragging = false;
    }
}
