using System;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    /// <summary>
    /// Runtime transform gizmo that attaches to a selected placeable.
    /// Axes are always aligned to world space regardless of object rotation.
    /// Scales with camera distance to remain consistently visible.
    /// </summary>
    public class TransformGizmo : MonoBehaviour
    {
        [Header("Handles")]
        [SerializeField]
        private Transform xHandle;

        [SerializeField]
        private Transform yHandle;

        [SerializeField]
        private Transform zHandle;

        [SerializeField]
        private Transform centerHandle;

        [SerializeField]
        private InputReader inputReader;

        [SerializeField]
        private Logging.Logger logger;

        [Header("Settings")]
        [SerializeField]
        private float screenSizeFactor = 0.1f;

        [SerializeField]
        private float boundsScaleFactor = 1.3f;

        [SerializeField]
        private LayerMask gizmoLayerMask;

        private Camera mainCamera;
        private Transform target;

        private enum DragAxis
        {
            None,
            X,
            Y,
            Z,
            Free,
        }

        private DragAxis activeDrag = DragAxis.None;
        private Vector3 dragStartObjectPos;
        private Vector3 dragStartMouseWorld;
        private Plane dragPlane;
        public event Action<Vector3> OnPositionChanged;

        private bool isPrimaryHeld = false;

        public void Initialize(Transform target, Camera camera)
        {
            this.target = target;
            mainCamera = camera;
        }

        private void OnEnable()
        {
            if (inputReader == null)
                return;
            inputReader.PrimaryInteractionEvent += OnPrimaryDown;
            inputReader.PrimaryInteractionReleasedEvent += OnPrimaryUp;
        }

        private void OnDisable()
        {
            if (inputReader == null)
                return;
            inputReader.PrimaryInteractionEvent -= OnPrimaryDown;
            inputReader.PrimaryInteractionReleasedEvent -= OnPrimaryUp;
            activeDrag = DragAxis.None;
            isPrimaryHeld = false;
        }

        private void OnPrimaryDown()
        {
            logger.Log("Primary interaction started!!!");
            isPrimaryHeld = true;
            TryBeginDrag();
        }

        private void OnPrimaryUp()
        {
            isPrimaryHeld = false;
            activeDrag = DragAxis.None;
        }

        private void LateUpdate()
        {
            if (target == null || mainCamera == null)
                return;

            transform.position = target.position;
            transform.rotation = Quaternion.identity;

            float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
            transform.localScale = Vector3.one * (distance * screenSizeFactor);
        }

        private void Update()
        {
            if (target == null)
                return;
            if (isPrimaryHeld && activeDrag != DragAxis.None)
                ContinueDrag();
        }

        private void TryBeginDrag()
        {
            Ray ray = mainCamera.ScreenPointToRay(inputReader.CurrentMousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 10000f, gizmoLayerMask))
            {
                logger.Log("Gizmo handle hit: " + hit.collider.transform);
                return;
            }

            Transform hitHandle = hit.collider.transform;
            logger.Log("Gizmo handle hit: " + hitHandle.name);

            if (hitHandle == xHandle)
                BeginAxisDrag(DragAxis.X, Vector3.right);
            else if (hitHandle == yHandle)
                BeginAxisDrag(DragAxis.Y, Vector3.up);
            else if (hitHandle == zHandle)
                BeginAxisDrag(DragAxis.Z, Vector3.forward);
            else if (hitHandle == centerHandle)
                BeginFreeDrag();
        }

        private void BeginAxisDrag(DragAxis axis, Vector3 axisDirection)
        {
            activeDrag = axis;
            dragStartObjectPos = target.position;

            Vector3 planeNormal = Vector3
                .Cross(axisDirection, Vector3.Cross(mainCamera.transform.forward, axisDirection))
                .normalized;

            if (planeNormal == Vector3.zero)
                planeNormal = mainCamera.transform.forward;

            dragPlane = new Plane(planeNormal, target.position);
            dragStartMouseWorld = GetMouseWorldOnPlane(dragPlane);
        }

        private void BeginFreeDrag()
        {
            activeDrag = DragAxis.Free;
            dragStartObjectPos = target.position;
            dragPlane = new Plane(-mainCamera.transform.forward, target.position);
            dragStartMouseWorld = GetMouseWorldOnPlane(dragPlane);
        }

        private void ContinueDrag()
        {
            Vector3 currentMouseWorld = GetMouseWorldOnPlane(dragPlane);
            Vector3 delta = currentMouseWorld - dragStartMouseWorld;

            switch (activeDrag)
            {
                case DragAxis.X:
                    target.position =
                        dragStartObjectPos + Vector3.right * Vector3.Dot(delta, Vector3.right);
                    break;
                case DragAxis.Y:
                    target.position =
                        dragStartObjectPos + Vector3.up * Vector3.Dot(delta, Vector3.up);
                    break;
                case DragAxis.Z:
                    target.position =
                        dragStartObjectPos + Vector3.forward * Vector3.Dot(delta, Vector3.forward);
                    break;
                case DragAxis.Free:
                    target.position = dragStartObjectPos + delta;
                    break;
            }

            OnPositionChanged?.Invoke(target.position);
        }

        private Vector3 GetMouseWorldOnPlane(Plane plane)
        {
            Ray ray = mainCamera.ScreenPointToRay(inputReader.CurrentMousePosition);
            if (plane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);
            return dragStartMouseWorld;
        }
    }
}
