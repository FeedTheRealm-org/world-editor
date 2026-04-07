using System;
using FeedTheRealm.Gameplay.Inputs;
using UnityEngine;

namespace FeedTheRealm.UI.PlaceableEditor
{
    /// <summary>
    /// Runtime scale gizmo that attaches to a selected placeable.
    /// Dragging X/Y/Z scales on that axis, dragging center scales uniformly.
    /// </summary>
    public class RotationGizmo : BaseGizmo
    {
        [Header("Handles")]
        [SerializeField]
        private Transform xHandle;

        [SerializeField]
        private Transform yHandle;

        [SerializeField]
        private Transform zHandle;

        [Header("Settings")]
        [SerializeField]
        private float rotationSensitivity = 180f;

        public event Action<Vector3> OnRotationChanged;

        private enum RotateAxis
        {
            None,
            X,
            Y,
            Z,
        }

        private RotateAxis activeRotation = RotateAxis.None;
        private Vector3 dragStartMouseWorld;
        private Quaternion dragStartRotation;
        private Plane dragPlane;
        private Vector3 rotationAxis;

        protected override void ResetDrag() => activeRotation = RotateAxis.None;

        private void Update()
        {
            if (target == null)
                return;
            if (isPrimaryHeld && activeRotation != RotateAxis.None)
                ContinueDrag();
        }

        protected override void TryBeginDrag()
        {
            Ray ray = mainCamera.ScreenPointToRay(inputReader.CurrentMousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 10000f, gizmoLayerMask))
                return;

            Transform hitHandle = hit.collider.transform;
            while (hitHandle.parent != null && hitHandle.parent != transform)
                hitHandle = hitHandle.parent;

            if (hitHandle == xHandle)
                BeginRotationDrag(RotateAxis.X, Vector3.right);
            else if (hitHandle == yHandle)
                BeginRotationDrag(RotateAxis.Y, Vector3.up);
            else if (hitHandle == zHandle)
                BeginRotationDrag(RotateAxis.Z, Vector3.forward);
        }

        private void BeginRotationDrag(RotateAxis axis, Vector3 worldAxis)
        {
            activeRotation = axis;
            rotationAxis = worldAxis;
            dragStartRotation = target.rotation;

            // drag plane is perpendicular to the rotation axis
            dragPlane = new Plane(worldAxis, target.position);
            dragStartMouseWorld = GetMouseWorldOnPlane(dragPlane, target.position);
        }

        private void ContinueDrag()
        {
            Vector3 currentMouseWorld = GetMouseWorldOnPlane(dragPlane, dragStartMouseWorld);

            // get vectors from object center to start and current mouse positions
            Vector3 fromVec = (dragStartMouseWorld - target.position).normalized;
            Vector3 toVec = (currentMouseWorld - target.position).normalized;

            // signed angle between the two vectors around the rotation axis
            float angle =
                Vector3.SignedAngle(fromVec, toVec, rotationAxis) * rotationSensitivity / 180f;

            target.rotation = dragStartRotation * Quaternion.AngleAxis(angle, rotationAxis);

            OnRotationChanged?.Invoke(target.rotation.eulerAngles);
        }
    }
}
