using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySpawnInteraction : MonoBehaviour {
  [SerializeField] Camera cam;
  [SerializeField] LayerMask enemySpawnMask;

  EnemySpawnPlace currentHover;

  void Update() {
    Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemySpawnMask)) {
      EnemySpawnPlace spawn =
          hit.collider.GetComponent<EnemySpawnPlace>();

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
        spawn.OpenHUD();
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
