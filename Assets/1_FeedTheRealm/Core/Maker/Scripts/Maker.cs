using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FPController))]
public class Maker : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] FPController fpController;

    private Vector2 moveInput = Vector2.zero; // WASD / XY
    private float verticalInput = 0f;         // Up/Down
    private Vector2 lookInput = Vector2.zero;  // Mouse look


    void Awake() {
        // This is to ensure the PlayerInput uses Keyboard & Mouse by default
        // I'm not sure if this is correct, we should review the player input system
        var playerInput = GetComponent<PlayerInput>();
        playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
    }


    public void OnMove(InputValue value) {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (Mouse.current.middleButton.isPressed)
        {

            lookInput = value.Get<Vector2>();
        }
        else
        {
            lookInput = Vector2.zero;
        }

        fpController.LookInput = lookInput;
    }

    void Update()
    {
        // Check vertical keys
        verticalInput = 0f;
        if (Keyboard.current.spaceKey.isPressed)
            verticalInput += 1f; // Move up
        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            verticalInput -= 1f; // Move down

        // Apply combined movement
        fpController.MoveInput = new Vector3(moveInput.x, moveInput.y, verticalInput);
        
    }
    // TODO: check to remove this since we already set as required
    void OnValidate()
    {
        if (fpController == null)
            fpController = GetComponent<FPController>();
    }
}
