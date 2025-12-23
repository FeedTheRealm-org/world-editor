using UnityEngine;
using System.Threading.Tasks;

public class WorldObjectController : MonoBehaviour {

    [Header("World Object Transform")]
    [SerializeField] private Vector3 size = Vector3.one;
    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private Vector3 offset;

    [Header("World Object info")]
    [SerializeField] private string assetName;
    [SerializeField] private GameObject missingObjectPrefab;

    private Transform visualRoot;

    async void Awake() {
        ApplyRootTransform();
        await GltfHandler.Load(
            assetName,
            gameObject,
            missingObjectPrefab
        );
        ApplyVisualTransform();
    }

#if UNITY_EDITOR
    private void OnValidate() {
        ApplyRootTransform();
        ApplyVisualTransform();
    }
#endif
    /// <summary>
    /// Applies the root transform (size, position, rotation) to the world object.
    /// </summary>
    private void ApplyRootTransform() {
        transform.localScale = size;
        transform.localPosition = position;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Applies the visual transform (offset, rotation) to the visual root.
    /// </summary>
    private void ApplyVisualTransform() {
        if (visualRoot == null) return;

        visualRoot.localPosition = offset;
        visualRoot.localEulerAngles = rotation;
        visualRoot.localScale = Vector3.one;
    }
}
