using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Gameplay.Inputs;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace FTR.UI
{
    public class MenuController : MonoBehaviour
    {
        [Inject]
        protected EnableInputEvent enableInputEvent;

        [Inject]
        protected EnableExternalInputsEvent enableExternalInputsEvent;

        [Inject]
        protected EnableEditorEvent enableEditorEvent;

        [Inject]
        protected IObjectResolver resolver;

        [Inject]
        protected InputReader inputReader;

        [Inject]
        private CloseAllEvent closeAllEvent;

        [SerializeField]
        private bool AllowInputWhileOpen = false;

        private bool AllowExternalInputWhileOpen = false;

        private VisualElement root;
        private VisualElement container;

        void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            container = root.Q<VisualElement>("Container");

            container?.AddToClassList("container--hidden");

            root.schedule.Execute(() =>
                {
                    container?.RemoveFromClassList("container--hidden");
                    container?.AddToClassList("container--visible");
                })
                .ExecuteLater(16);

            if (enableInputEvent != null)
                enableInputEvent.OnRaised += OnInputEventRaised;

            if (enableInputEvent != null && !AllowInputWhileOpen)
                enableInputEvent.Raise(false);

            if (enableExternalInputsEvent != null)
                enableExternalInputsEvent.OnRaised += OnExternalInputEventRaised;

            if (enableExternalInputsEvent != null && !AllowExternalInputWhileOpen)
                enableExternalInputsEvent.Raise(false);

            enableEditorEvent?.Raise(false);
            closeAllEvent.OnRaised += CloseMenu;
            inputReader.CloseMenuEvent += CloseMenu;
        }

        void OnDestroy()
        {
            enableInputEvent.OnRaised -= OnInputEventRaised;
            enableExternalInputsEvent.OnRaised -= OnExternalInputEventRaised;
            closeAllEvent.OnRaised -= CloseMenu;
            inputReader.CloseMenuEvent -= CloseMenu;
        }

        private void OnInputEventRaised(bool isEnabled)
        {
            if (isEnabled && !AllowInputWhileOpen)
                enableInputEvent?.Raise(false);
        }

        private void OnExternalInputEventRaised(bool isEnabled)
        {
            if (isEnabled && !AllowExternalInputWhileOpen)
                enableExternalInputsEvent?.Raise(false);
        }

        public virtual void CloseMenu()
        {
            container?.RemoveFromClassList("container--visible");
            container?.AddToClassList("container--hidden");

            if (enableInputEvent != null)
                enableInputEvent.OnRaised -= OnInputEventRaised;

            if (enableExternalInputsEvent != null)
                enableExternalInputsEvent.OnRaised -= OnExternalInputEventRaised;

            enableInputEvent?.Raise(true);
            enableExternalInputsEvent?.Raise(true);
            enableEditorEvent?.Raise(true);

            // only one destroy, after animation completes
            root?.schedule.Execute(() => Destroy(gameObject)).ExecuteLater(200);
        }

        public virtual void OpenMenu(GameObject menuPrefab)
        {
            resolver.Instantiate(menuPrefab);
            Destroy(gameObject);
        }

        public void EnableMovementToggle(bool enableMovement)
        {
            enableExternalInputsEvent.Raise(enableMovement);
        }
    }
}
