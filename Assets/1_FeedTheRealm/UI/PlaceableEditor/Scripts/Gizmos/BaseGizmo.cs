using FeedTheRealm.Gameplay.Inputs;
using UnityEngine;

namespace FeedTheRealm.UI.PlaceableEditor
{
    public abstract class BaseGizmo : MonoBehaviour
    {
        [SerializeField]
        protected InputReader inputReader;

        [SerializeField]
        protected Logging.Logger logger;

        [SerializeField]
        protected float screenSizeFactor = 0.1f;

        [SerializeField]
        protected LayerMask gizmoLayerMask;

        protected Camera mainCamera;
        protected Transform target;
        protected bool isPrimaryHeld = false;

        public void Initialize(Transform target, Camera camera)
        {
            this.target = target;
            this.mainCamera = camera;
        }

        protected virtual void OnEnable()
        {
            if (inputReader == null)
                return;
            inputReader.PrimaryInteractionEvent += OnPrimaryDown;
            inputReader.PrimaryInteractionReleasedEvent += OnPrimaryUp;
        }

        protected virtual void OnDisable()
        {
            if (inputReader == null)
                return;
            inputReader.PrimaryInteractionEvent -= OnPrimaryDown;
            inputReader.PrimaryInteractionReleasedEvent -= OnPrimaryUp;
            isPrimaryHeld = false;
        }

        protected virtual void LateUpdate()
        {
            if (target == null || mainCamera == null)
                return;
            transform.position = target.position;
            transform.rotation = Quaternion.identity;
            float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
            transform.localScale = Vector3.one * (distance * screenSizeFactor);
        }

        protected virtual void OnPrimaryDown()
        {
            isPrimaryHeld = true;
            TryBeginDrag();
        }

        protected virtual void OnPrimaryUp()
        {
            isPrimaryHeld = false;
            ResetDrag();
        }

        protected abstract void TryBeginDrag();
        protected abstract void ResetDrag();

        protected Vector3 GetMouseWorldOnPlane(Plane plane, Vector3 fallback)
        {
            Ray ray = mainCamera.ScreenPointToRay(inputReader.CurrentMousePosition);
            if (plane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);
            return fallback;
        }
    }
}
