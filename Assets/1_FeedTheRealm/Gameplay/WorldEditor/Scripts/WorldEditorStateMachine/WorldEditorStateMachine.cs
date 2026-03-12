using System;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine.WorldEditorStates;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine
{
    public class WorldEditorStateMachine : MonoBehaviour
    {
        [SerializeField]
        private Logging.Logger logger;

        [Inject]
        private ObjectSelectedEvent objectSelectedEvent;

        [Inject]
        private EnableEditorEvent enableEditorEvent;

        public InputReader inputReader;
        public Camera playerCamera;
        public IPlaceable SelectedObject { get; private set; }
        public bool IsEditorEnabled { get; private set; } = true;
        private IWorldEditorState currentState;

        // ------ States ------
        public SelectingState SelectingState { get; private set; }
        public PlacingState PlacingState { get; private set; }
        public RemovingState RemovingState { get; private set; }
        public EditingState EditingState { get; private set; }

        // -------------------- Public Methods --------------------

        void Start()
        {
            IsEditorEnabled = true;
            SelectingState = new SelectingState(this);
            PlacingState = new PlacingState(this);
            RemovingState = new RemovingState(this);
            SetState(SelectingState);
        }

        public void SetState(IWorldEditorState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        public void OnPrimaryInteraction()
        {
            if (!IsEditorEnabled)
                return;
            currentState?.OnPrimaryAction();
        }

        public void OnSecondaryInteraction()
        {
            if (!IsEditorEnabled)
                return;
            currentState?.OnSecondaryAction();
        }

        public void OnRemoveAction()
        {
            SetState(new RemovingState(this));
        }

        public void Log(string message, Logging.LogType type = Logging.LogType.Info)
        {
            logger.Log(message, this, type);
        }

        // -------------------- Private Methods --------------------

        private void OnEnable()
        {
            inputReader.PrimaryInteractionEvent += OnPrimaryInteraction;
            inputReader.SecondaryInteractionEvent += OnSecondaryInteraction;
            if (objectSelectedEvent != null)
                objectSelectedEvent.OnRaised += OnWorldObjectSelected;
            if (enableEditorEvent != null)
                enableEditorEvent.OnRaised += ToggleEditor;
            inputReader.RemoveEvent += OnRemoveAction;
        }

        private void OnDisable()
        {
            inputReader.PrimaryInteractionEvent -= OnPrimaryInteraction;
            inputReader.SecondaryInteractionEvent -= OnSecondaryInteraction;
            if (objectSelectedEvent != null)
                objectSelectedEvent.OnRaised -= OnWorldObjectSelected;
            if (enableEditorEvent != null)
                enableEditorEvent.OnRaised -= ToggleEditor;
            inputReader.RemoveEvent -= OnRemoveAction;
        }

        private void ToggleEditor(bool enabled)
        {
            IsEditorEnabled = enabled;
            if (!enabled)
                currentState?.Exit();
            else
                SetState(SelectingState);
            Debug.Log($"Interaction {(enabled ? "enabled" : "disabled")}.");
        }

        private void OnWorldObjectSelected(IPlaceable reference)
        {
            if (currentState is PlacingState)
            {
                SelectedObject = reference;
                Log($"Switched to: {reference.DisplayName}");
                return;
            }
            SelectedObject = reference;
            SetState(new PlacingState(this));
        }

        private void Update()
        {
            if (!IsEditorEnabled)
                return;
            currentState?.Tick();
        }
    }
}
