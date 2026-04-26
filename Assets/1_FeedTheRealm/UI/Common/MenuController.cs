using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.UI.Common
{
    public class MenuController : MonoBehaviour
    {
        [Inject]
        protected EnableInputEvent enableInputEvent;

        [Inject]
        protected EnableEditorEvent enableEditorEvent;

        [Inject]
        protected IObjectResolver resolver;

        [SerializeField]
        private bool AllowInputWhileOpen = false;

        void Awake()
        {
            if (enableInputEvent != null)
                enableInputEvent.OnRaised += OnInputEventRaised;

            if (enableInputEvent != null && !AllowInputWhileOpen)
                enableInputEvent.Raise(false);

            enableEditorEvent?.Raise(false);
        }

        void OnDestroy()
        {
            if (enableInputEvent != null)
                enableInputEvent.OnRaised -= OnInputEventRaised;
        }

        private void OnInputEventRaised(bool isEnabled)
        {
            if (isEnabled && !AllowInputWhileOpen)
                enableInputEvent?.Raise(false);
        }

        public virtual void CloseMenu()
        {
            if (enableInputEvent != null)
                enableInputEvent.OnRaised -= OnInputEventRaised;
            enableInputEvent?.Raise(true);
            enableEditorEvent?.Raise(true);
            Destroy(gameObject);
        }

        public virtual void OpenMenu(GameObject menuPrefab)
        {
            resolver.Instantiate(menuPrefab);
            Destroy(gameObject);
        }

        public void EnableMovementToggle(bool enableMovement)
        {
            enableInputEvent.Raise(enableMovement);
        }
    }
}
