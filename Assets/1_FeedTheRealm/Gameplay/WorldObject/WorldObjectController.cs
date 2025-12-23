using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class WorldObjectController : MonoBehaviour
{
    [Header("World Object Transform")]
    [SerializeField]
    private string id;

    [SerializeField]
    private Vector3 size = Vector3.one;

    [SerializeField]
    private Vector3 position;

    [SerializeField]
    private Vector3 rotation;

    [SerializeField]
    private Vector3 offset;

    [Header("World Object info")]
    [Tooltip(
        "When using local assets, this is the asset name without extension from the StreamingAssets folder. For remote assets, this is the full URL."
    )]
    [SerializeField]
    private string objectUrl;

    [SerializeField]
    private bool isLocalAsset = true;

    [SerializeField]
    private GameObject missingObjectPrefab;

    [Header("Debugging")]
    [SerializeField]
    [Tooltip("If you want to render the game object in the current scene, set this to true")]
    private bool instantiateObject = false;
    private bool isObjectLoaded = false;
    private GameObject worldObject;
    private Transform visualRoot;

    public string Id
    {
        get => id;
        set => id = value;
    }

    public Vector3 Size
    {
        get => size;
        set => size = value;
    }

    public Vector3 Position
    {
        get => position;
        set => position = value;
    }

    public Vector3 Rotation
    {
        get => rotation;
        set => rotation = value;
    }

    public string DisplayName
    {
        get => objectUrl;
    }

    public GameObject GetObject()
    {
        return Instantiate(worldObject);
    }

    async void Awake()
    {
        // this is for testing by placing the game object in the scene directly
        if (instantiateObject)
            worldObject = gameObject;
        await LoadObject();
        ApplyRootTransform();
        ApplyVisualTransform();
    }

    private void OnValidate()
    {
        ApplyRootTransform();
        ApplyVisualTransform();
    }

    /// <summary>
    /// Applies the root transform (size, position, rotation) to the world object.
    /// </summary>
    private void ApplyRootTransform()
    {
        worldObject.transform.localScale = size;
        worldObject.transform.localPosition = position;
        worldObject.transform.localRotation = Quaternion.identity;
        visualRoot = worldObject.transform.GetChild(0);
    }

    /// <summary>
    /// Applies the visual transform (offset, rotation) to the visual root.
    /// </summary>
    private void ApplyVisualTransform()
    {
        if (visualRoot == null)
            return;
        visualRoot.localPosition = offset;
        visualRoot.localEulerAngles = rotation;
        visualRoot.localScale = Vector3.one;
    }

    private async Task LoadObject()
    {
        if (isObjectLoaded)
            return;
        await GltfHandler.Load(objectUrl, worldObject, missingObjectPrefab, isLocalAsset);
        isObjectLoaded = true;
    }
}
