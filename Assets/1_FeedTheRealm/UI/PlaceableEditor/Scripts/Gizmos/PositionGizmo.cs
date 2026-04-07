using System;
using UnityEngine;

namespace FeedTheRealm.UI.PlaceableEditor
{
    /// <summary>
    /// Runtime transform gizmo that attaches to a selected placeable.
    /// Axes are always aligned to world space regardless of object rotation.
    /// Scales with camera distance to remain consistently visible.
    /// </summary>
    public class PositionGizmo : BaseGizmo
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

        public event Action<Vector3> OnPositionChanged;

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

        protected override void OnPrimaryDown()
        {
            base.OnPrimaryDown();
        }

        protected override void OnPrimaryUp()
        {
            base.OnPrimaryUp();
        }

        protected override void ResetDrag() => activeDrag = DragAxis.None;

        private void Update()
        {
            if (target == null)
                return;
            if (isPrimaryHeld && activeDrag != DragAxis.None)
                ContinueDrag();
        }

        protected override void TryBeginDrag()
        {
            Ray ray = mainCamera.ScreenPointToRay(inputReader.CurrentMousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 10000f, gizmoLayerMask))
                return;

            Transform hitHandle = hit.collider.transform;
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
            dragStartMouseWorld = GetMouseWorldOnPlane(dragPlane, target.position);
        }

        private void BeginFreeDrag()
        {
            activeDrag = DragAxis.Free;
            dragStartObjectPos = target.position;
            dragPlane = new Plane(-mainCamera.transform.forward, target.position);
            dragStartMouseWorld = GetMouseWorldOnPlane(dragPlane, target.position);
        }

        private void ContinueDrag()
        {
            Vector3 currentMouseWorld = GetMouseWorldOnPlane(dragPlane, dragStartMouseWorld);
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
    }
}
