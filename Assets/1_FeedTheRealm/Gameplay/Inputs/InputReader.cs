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
        public event Action<Vector2> MoveEvent;
        public event Action<Vector2> LookEvent;
        public event Action PrimaryInteractionEvent;
        public event Action SecondaryInteractionEvent;
        public event Action<float> MoveVerticalEvent;
        public event Action RemoveEvent;
        public event Action<Vector2> ScrollEvent;
        public Vector2 LastClickPosition { get; private set; }

        private MakerControls controls;

        private void OnEnable()
        {
            if (controls == null)
            {
                controls = new MakerControls();
                controls.Player.SetCallbacks(this);
            }
            controls.Player.Enable();
        }

        private void OnDisable()
        {
            controls.Player.Disable();
            if (enableInputEvent != null)
                enableInputEvent.OnRaised -= ToggleInput;
        }

        public void ToggleInput(bool isEnabled)
        {
            if (isEnabled)
            {
                controls.Player.Move.Enable();
                controls.Player.Look.Enable();
                controls.Player.MoveUp.Enable();
                controls.Player.MoveDown.Enable();
                controls.Player.PrimaryInteraction.Enable();
                controls.Player.SecondaryInteraction.Enable();
            }
            else
            {
                controls.Player.Move.Disable();
                controls.Player.Look.Disable();
                controls.Player.MoveUp.Disable();
                controls.Player.MoveDown.Disable();
                controls.Player.PrimaryInteraction.Disable();
                controls.Player.SecondaryInteraction.Disable();
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
                {
                    LastClickPosition = Mouse.current.position.ReadValue();
                }
                PrimaryInteractionEvent?.Invoke();
            }
        }

        public void OnSecondaryInteraction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SecondaryInteractionEvent?.Invoke();
            }
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
    }
}
