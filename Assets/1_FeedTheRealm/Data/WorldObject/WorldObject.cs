using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class WorldObjectReference
{
    public string id;
    public Vector3 size = Vector3.one;
    public Vector3 rotation;
    public Vector3 offset;
    public string objectUrl;

    // MOVE THIS TO ANOTHER COMPONENT
    public bool isLocalAsset = true;

    private bool isObjectLoaded = false;
    private GameObject worldObject;
    private Transform visualRoot;

    /// <summary>
    /// Sets up the world object with the given parameters.
    /// This is destined to be called before GetObject().
    /// </summary>
    public WorldObjectReference(
        string id,
        string objectUrl,
        Vector3 size,
        Vector3 rotation,
        Vector3 offset,
        bool isLocalAsset = true
    )
    {
        this.id = id;
        this.objectUrl = objectUrl;
        this.size = size;
        this.rotation = rotation;
        this.offset = offset;
        this.isLocalAsset = isLocalAsset;
    }

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

    public Vector3 Rotation
    {
        get => rotation;
        set => rotation = value;
    }

    public string DisplayName
    {
        get
        {
            var name = objectUrl.Replace("_", " ");
            var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(name);
        }
    }

    public string ObjectUrl
    {
        get => objectUrl;
        set => objectUrl = value;
    }

    /// <summary>
    /// Asynchronously gets the world object instance with the applied transform.
    /// </summary>
    public async Task<GameObject> CreateWorldObjectInstance(int layerMask)
    {
        GameObject instance;
        if (!isObjectLoaded)
        {
            Debug.Log("World object not loaded yet. Loading object...");
            await LoadWorldObject();
        }
        instance = Object.Instantiate(worldObject);
        instance.SetActive(true);
        ApplyTransform(instance, layerMask);
        return instance;
    }

    private async Task LoadWorldObject()
    {
        worldObject = new GameObject($"Loaded_{DisplayName}");
        await GltfHandler.Load(worldObject, objectUrl, isLocalAsset);
        isObjectLoaded = true;

        foreach (Transform child in worldObject.transform)
        {
            child.gameObject.AddComponent<BoxCollider>();
        }
        worldObject.name = DisplayName;
        worldObject.SetActive(false);
    }

    /// <summary>
    /// Applies the transform (size, position, rotation) to the instance of the world object.
    /// </summary>
    private void ApplyTransform(GameObject instance, int layerMask)
    {
        if (instance == null)
        {
            Debug.LogError("Instance is null in ApplyTransform");
            return;
        }
        instance.transform.localScale = size;
        instance.transform.localRotation = Quaternion.identity;
        if (instance.transform.childCount > 0)
        {
            visualRoot = instance.transform.GetChild(0);
            visualRoot.localPosition = offset;
            visualRoot.localEulerAngles = rotation;
            visualRoot.localScale = Vector3.one;
        }
        SetLayerRecursively(instance, layerMask); //TODO: check if this can be set in the loader insted
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void AddCollidersRecursively(GameObject obj)
    {
        obj.AddComponent<BoxCollider>();
        foreach (Transform child in obj.transform)
        {
            AddCollidersRecursively(child.gameObject);
        }
    }
}
