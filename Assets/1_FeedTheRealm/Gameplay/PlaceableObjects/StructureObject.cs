using System.Threading.Tasks;
using Models;
using UnityEngine;

[System.Serializable]
public class StructureObject : IPlaceable
{
    public string id;
    public Vector3 size = Vector3.one;
    public Vector3 rotation;
    public Vector3 offset;
    public string objectUrl;
    private bool isObjectLoaded = false;
    private GameObject worldObject;
    private Transform visualRoot;

    public StructureObject(
        string id,
        Vector3 size,
        Vector3 rotation,
        Vector3 offset,
        string objectUrl
    )
    {
        this.id = id;
        this.size = size;
        this.rotation = rotation;
        this.offset = offset;
        this.objectUrl = objectUrl;
    }

    public string DisplayName => objectUrl.Replace("_", " ").Replace("-", " ").ToUpper();

    public async Task<GameObject> GetPlaceableObject(int layerMask)
    {
        GameObject instance;
        if (!isObjectLoaded)
        {
            await LoadWorldObject();
        }
        instance = Object.Instantiate(worldObject);
        instance.SetActive(true);
        ApplyTransform(instance, layerMask);
        return instance;
    }

    // -------------------- Private Methods --------------------

    private async Task LoadWorldObject()
    {
        worldObject = new GameObject($"Loaded_{DisplayName}");
        await API.GltfHandler.Load(worldObject, objectUrl);
        isObjectLoaded = true;

        foreach (Transform child in worldObject.transform)
        {
            child.gameObject.AddComponent<BoxCollider>();
        }
        worldObject.name = id;

        StructureController controller = worldObject.AddComponent<StructureController>();
        controller.objectName = objectUrl;
        controller.id = id;
        controller.size = size;
        controller.rotation = rotation;
        controller.offset = offset;
        worldObject.SetActive(false);
    }

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
        SetLayerRecursively(instance, layerMask);
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
