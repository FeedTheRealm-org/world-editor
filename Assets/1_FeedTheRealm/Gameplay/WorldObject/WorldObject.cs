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
        get => objectUrl;
    }

    public async Task<GameObject> GetWorldObject()
    {
        await LoadModel();
        ApplyTransform(worldObject);
        return worldObject;
    }

    /// <summary>
    /// Applies the transform (size, position, rotation) to the instance of the world object.
    /// </summary>
    private void ApplyTransform(GameObject instance = null)
    {
        instance.transform.localScale = size;
        instance.transform.localRotation = Quaternion.identity;
        visualRoot = instance.transform.GetChild(0);
        visualRoot.localPosition = offset;
        visualRoot.localEulerAngles = rotation;
        visualRoot.localScale = Vector3.one;
    }

    private async Task LoadModel()
    {
        if (isObjectLoaded)
            return;
        await GltfHandler.Load(objectUrl, worldObject, isLocalAsset);
        isObjectLoaded = true;
    }
}
