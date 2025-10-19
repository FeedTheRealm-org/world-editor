using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FPController))]
public class Player : MonoBehaviour {
    [Header("Components")]
    [SerializeField] FPController fpController;

    #region Input Handleing

    void OnMove(InputValue value) {
        fpController.MoveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value) {
        fpController.LookInput = value.Get<Vector2>();
    }


    #endregion

    #region Unity Methods

    void OnValidate() {
        if (fpController == null)
            fpController = GetComponent<FPController>();
    }
    #endregion

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

    }

    // Update is called once per frame
    void Update() {

    }
}
