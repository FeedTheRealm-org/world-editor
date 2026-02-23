using UnityEngine;

[CreateAssetMenu(fileName = "MakerConfig", menuName = "Scriptable Objects/MakerConfig")]
public class MovementConfig : ScriptableObject
{
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float verticalSpeed = 3f;

    // CameraController configs
    public Vector2 lookSensitivity = new Vector2(1f, 1f);
    public float pitchLimit = 85f;
}
