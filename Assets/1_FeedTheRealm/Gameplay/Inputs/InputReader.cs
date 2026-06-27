using System;
using FeedTheRealm.Core.EventChannels.UIEvents;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace FeedTheRealm.Gameplay.Inputs
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
    public class InputReader : ScriptableObject, MakerControls.IPlayerActions
    {
        [SerializeField]
        private EnableInputEvent enableInputEvent;

        [SerializeField]
        private EnableExternalInputsEvent enableMovementEvent;
        public event Action<Vector2> MoveEvent;
        public event Action<Vector2> LookEvent;
        public event Action PrimaryInteractionEvent;
        public event Action SecondaryInteractionEvent;
        public event Action<float> MoveVerticalEvent;
        public event Action RemoveEvent;
        public event Action MoveShortcutEvent;
        public event Action RotateShortcutEvent;
        public event Action ScaleShortcutEvent;
        public event Action HideShortcutEvent;
        public event Action ColliderShortcutEvent;
        public event Action<Vector2> ScrollEvent;
        public Vector2 LastClickPosition { get; private set; }
        public event Action PrimaryInteractionReleasedEvent;
        public event Action<Vector2> MousePositionEvent;
        public Vector2 CurrentMousePosition { get; private set; }
        public event Action CloseMenuEvent;

        private MakerControls controls;

        private void OnEnable()
        {
            if (controls == null)
            {
                controls = new MakerControls();
                controls.Player.SetCallbacks(this);
            }
            controls.Player.Enable();
            enableInputEvent.OnRaised += ToggleInput;
            enableMovementEvent.OnRaised += ToggleExternalInputs;
        }

        private void OnDisable()
        {
            controls.Player.Disable();
            if (enableInputEvent != null)
                enableInputEvent.OnRaised -= ToggleInput;
            if (enableMovementEvent != null)
                enableMovementEvent.OnRaised -= ToggleExternalInputs;
        }

        public void ToggleExternalInputs(bool isEnabled)
        {
            if (isEnabled)
            {
                controls.Player.RemoveAction.Enable();
            }
            else
            {
                controls.Player.RemoveAction.Disable();
            }
        }

        public void ToggleInput(bool isEnabled)
        {
            if (isEnabled)
            {
                controls.Player.SecondaryInteraction.Enable();
                controls.Player.PrimaryInteraction.Enable();
                controls.Player.Move.Enable();
                controls.Player.MoveUp.Enable();
                controls.Player.Look.Enable();
                controls.Player.MoveDown.Enable();
            }
            else
            {
                controls.Player.Move.Disable();
                controls.Player.MoveUp.Disable();
                controls.Player.Look.Disable();
                controls.Player.MoveDown.Disable();
                controls.Player.SecondaryInteraction.Disable();
                controls.Player.PrimaryInteraction.Disable();
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MoveEvent?.Invoke(context.ReadValue<Vector2>());
            }
            else if (context.canceled)
            {
                MoveEvent?.Invoke(Vector2.zero);
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                LookEvent?.Invoke(context.ReadValue<Vector2>());
            }
            else if (context.canceled)
            {
                LookEvent?.Invoke(Vector2.zero);
            }
        }

        public void OnMoveUp(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MoveVerticalEvent?.Invoke(context.ReadValue<float>());
            }
            else if (context.canceled)
            {
                MoveVerticalEvent?.Invoke(0f);
            }
        }

        public void OnMoveDown(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MoveVerticalEvent?.Invoke(context.ReadValue<float>());
            }
            else if (context.canceled)
            {
                MoveVerticalEvent?.Invoke(0f);
            }
        }

        public void OnPrimaryInteraction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (Mouse.current != null)
                    LastClickPosition = Mouse.current.position.ReadValue();
                PrimaryInteractionEvent?.Invoke();
            }
            else if (context.canceled)
            {
                PrimaryInteractionReleasedEvent?.Invoke();
            }
        }

        public void OnSecondaryInteraction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SecondaryInteractionEvent?.Invoke();
            }
        }

        public void RaiseSecondaryInteraction()
        {
            SecondaryInteractionEvent?.Invoke();
        }

        public void OnRemoveAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                RemoveEvent?.Invoke();
            }
        }

        public void OnScroll(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ScrollEvent?.Invoke(context.ReadValue<Vector2>());
            }
        }

        public void OnCursorPosition(InputAction.CallbackContext context)
        {
            CurrentMousePosition = context.ReadValue<Vector2>();
        }

        public void OnMoveSC(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MoveShortcutEvent?.Invoke();
            }
        }

        public void OnScaleSC(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ScaleShortcutEvent?.Invoke();
            }
        }

        public void OnRotateSC(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                RotateShortcutEvent?.Invoke();
            }
        }

        public void OnHideSC(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                HideShortcutEvent?.Invoke();
            }
        }

        public void OnColliderSC(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ColliderShortcutEvent?.Invoke();
            }
        }

        public void OnCloseMenu(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                CloseMenuEvent?.Invoke();
            }
        }
    }
}
