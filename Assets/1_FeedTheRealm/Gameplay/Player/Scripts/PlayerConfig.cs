using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Scriptable Objects/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public float positionX = 0f;
    public float positionY = 20f;
    public float positionZ = 0f;
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float verticalSpeed = 3f;

    // CameraController configs
    public Vector2 lookSensitivity = new Vector2(1f, 1f);
    public float pitchLimit = 85f;
}
