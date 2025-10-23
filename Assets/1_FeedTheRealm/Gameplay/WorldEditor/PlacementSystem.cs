using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject placementIndicator;

    [SerializeField]
    private InputManager inputManager;


    void Update() {
        Vector3 placementPosition = inputManager.GetSelectedMapPosition();
        placementIndicator.transform.position = placementPosition;
    }
}
