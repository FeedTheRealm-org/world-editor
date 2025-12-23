using UnityEngine;
using UnityEngine.InputSystem;

public class ViewInfoManager : MonoBehaviour {
  [SerializeField]
  private Camera mainCamera;

  [SerializeField]
  private LayerMask placementLayerMask;

  void Update() {
    Vector3 mousePosition = Mouse.current.position.ReadValue();
    mousePosition.z = mainCamera.nearClipPlane;
    Ray ray = mainCamera.ScreenPointToRay(mousePosition);

    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayerMask)) {
      EnemySpawnPlaceController spawnPlace = hit.collider.GetComponent<EnemySpawnPlaceController>();
      if (spawnPlace != null) {
        spawnPlace.Select();
      }
    }
  }
}
