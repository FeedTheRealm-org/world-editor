using UnityEngine;
using UnityEngine.InputSystem;

public class ViewInfoManager : MonoBehaviour {
  [SerializeField] Camera mainCamera;
  [SerializeField] LayerMask enemySpawnMask;
  EnemySpawnPlace currentHover;

  void Update() {
    Vector3 mousePosition = Mouse.current.position.ReadValue();
    mousePosition.z = mainCamera.nearClipPlane;
    Ray ray = mainCamera.ScreenPointToRay(mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemySpawnMask)) {
      EnemySpawnPlace spawn = hit.collider.GetComponent<EnemySpawnPlace>();

      if (spawn == null) {
        ClearHover();
        return;
      }

      if (spawn != currentHover) {
        ClearHover();
        currentHover = spawn;
        currentHover.Highlight(true);
      }

      if (Mouse.current.rightButton.wasPressedThisFrame) {
        currentHover.OpenHUD();
      }

    } else {
      ClearHover();
    }
  }

  void ClearHover() {
    if (currentHover != null) {
      currentHover.Highlight(false);
      currentHover = null;
    }
  }
}
