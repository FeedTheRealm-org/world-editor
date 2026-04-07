using System;
using FeedTheRealm.Gameplay.Inputs;
using UnityEngine;

namespace FeedTheRealm.UI.PlaceableEditor
{
    /// <summary>
    /// Runtime scale gizmo that attaches to a selected placeable.
    /// Dragging X/Y/Z scales on that axis, dragging center scales uniformly.
    /// </summary>
    public class ScaleGizmo : BaseGizmo
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

        [Header("Settings")]
        [SerializeField]
        private float scaleSensitivity = 1f;

        public event Action<Vector3> OnScaleChanged;

        private enum ScaleAxis
        {
            None,
            X,
            Y,
            Z,
            Uniform,
        }

        private ScaleAxis activeScale = ScaleAxis.None;
        private Vector3 dragStartScale;
        private Vector3 dragStartMouseWorld;
        private Plane dragPlane;

        protected override void ResetDrag() => activeScale = ScaleAxis.None;

        private void Update()
        {
            if (target == null)
                return;
            if (isPrimaryHeld && activeScale != ScaleAxis.None)
                ContinueDrag();
        }

        protected override void TryBeginDrag()
        {
            Ray ray = mainCamera.ScreenPointToRay(inputReader.CurrentMousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 10000f, gizmoLayerMask))
            {
                return;
            }

            Transform hitHandle = hit.collider.transform;
            while (hitHandle.parent != null && hitHandle.parent != transform)
                hitHandle = hitHandle.parent;
            if (hitHandle == xHandle)
                BeginScaleDrag(ScaleAxis.X, Vector3.right);
            else if (hitHandle == yHandle)
                BeginScaleDrag(ScaleAxis.Y, Vector3.up);
            else if (hitHandle == zHandle)
                BeginScaleDrag(ScaleAxis.Z, Vector3.forward);
            else if (hitHandle == centerHandle)
                BeginScaleDrag(ScaleAxis.Uniform, mainCamera.transform.right);
        }

        private void BeginScaleDrag(ScaleAxis axis, Vector3 dragDirection)
        {
            activeScale = axis;
            dragStartScale = target.localScale;

            Vector3 planeNormal;

            if (axis == ScaleAxis.Uniform)
            {
                // camera facing plane for uniform scale
                planeNormal = -mainCamera.transform.forward;
            }
            else
            {
                // build plane containing the axis and facing the camera
                // same technique as TransformGizmo axis drag
                planeNormal = Vector3
                    .Cross(
                        dragDirection,
                        Vector3.Cross(mainCamera.transform.forward, dragDirection)
                    )
                    .normalized;

                if (planeNormal == Vector3.zero)
                    planeNormal = mainCamera.transform.forward;
            }

            dragPlane = new Plane(planeNormal, target.position);
            dragStartMouseWorld = GetMouseWorldOnPlane(dragPlane, target.position);
        }

        private void ContinueDrag()
        {
            Vector3 currentMouseWorld = GetMouseWorldOnPlane(dragPlane, dragStartMouseWorld);
            Vector3 delta = currentMouseWorld - dragStartMouseWorld;

            switch (activeScale)
            {
                case ScaleAxis.X:
                    float deltaX = Vector3.Dot(delta, Vector3.right) * scaleSensitivity;
                    target.localScale = dragStartScale + new Vector3(deltaX, 0, 0);
                    break;
                case ScaleAxis.Y:
                    float deltaY = Vector3.Dot(delta, Vector3.up) * scaleSensitivity;
                    target.localScale = dragStartScale + new Vector3(0, deltaY, 0);
                    break;
                case ScaleAxis.Z:
                    float deltaZ = Vector3.Dot(delta, Vector3.forward) * scaleSensitivity;
                    target.localScale = dragStartScale + new Vector3(0, 0, deltaZ);
                    break;
                case ScaleAxis.Uniform:
                    float uniformDelta =
                        Vector3.Dot(delta, mainCamera.transform.right) * scaleSensitivity;
                    target.localScale = dragStartScale + Vector3.one * uniformDelta;
                    break;
            }

            target.localScale = new Vector3(
                Mathf.Max(0.01f, target.localScale.x),
                Mathf.Max(0.01f, target.localScale.y),
                Mathf.Max(0.01f, target.localScale.z)
            );

            OnScaleChanged?.Invoke(target.localScale);
        }
    }
}
