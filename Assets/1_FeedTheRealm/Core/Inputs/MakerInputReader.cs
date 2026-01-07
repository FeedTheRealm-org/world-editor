using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "MakerInputReader", menuName = "Scriptable Objects/MakerInputReader")]
public class MakerInputReader : ScriptableObject, MakerControls.IPlayerActions
{
    public event Action<Vector2> MoveEvent;
    public event Action<Vector2> LookEvent;
    public event Action PrimaryInteractionEvent;
    public event Action SecondaryInteractionEvent;
    public event Action<float> MoveVerticalEvent;
    public event Action RemoveEvent;
    public event Action CursorToggleEvent;
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
    }

    public void ToggleInput(bool isEnabled)
    {
        if (isEnabled)
            controls.Player.Enable();
        else
            controls.Player.Disable();
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

    public void OnCursorToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CursorToggleEvent?.Invoke();
        }
    }
}
