using UnityEngine;
using UnityEngine.Events;

public class EnemySpawnPlaceController : MonoBehaviour {
    public UnityEvent OnSelected;

    public void Select() {
        Debug.Log("EnemySpawnPlace selected");
        OnSelected?.Invoke();
    }
}