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
    [SerializeField] private float scrollSpeed = 5f;       // How fast zoom changes with scroll
    [SerializeField] private float zoomSmoothness = 10f;   // How smooth the zoom transition feels
    [SerializeField] private float minDistance = 1f;       // Minimum zoom distance
    [SerializeField] private float maxDistance = 200f;     // Maximum zoom distance

    [Header("Drag Settings")]
    [SerializeField] private bool smoothDrag = false;      // Optionally smooth dragging movement
    [SerializeField] private float dragSmoothness = 15f;   // Only used if smoothDrag = true

    void Start() {
        mainCamera = Camera.main;
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
                Debug.Log($"Started dragging {gameObject.name}");
            }
        }
    }

    private void HandleScroll() {
        float scrollDelta = -Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollDelta) > 0.01f) {
            targetDistance -= scrollDelta * scrollSpeed * Time.deltaTime;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // Smoothly interpolate toward the target distance
        distanceFromCamera = Mathf.Lerp(distanceFromCamera, targetDistance, Time.deltaTime * zoomSmoothness);
    }

    private void UpdateDrag() {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 targetPoint = ray.GetPoint(distanceFromCamera);
        Vector3 desiredPosition = targetPoint + offset;

        if (smoothDrag)
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * dragSmoothness);
        else
            transform.position = desiredPosition;
    }

    private void StopDragging() {
        isDragging = false;
        Debug.Log($"Stopped dragging {gameObject.name}");
    }
}
