using System.Threading.Tasks;
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
    public GameObject structureObject;
    private GameObject worldObject;

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
        if (!isObjectLoaded)
            await LoadWorldObject();
        GameObject structureInstance = Object.Instantiate(structureObject);
        structureInstance.transform.position = Vector3.zero;
        structureInstance.transform.rotation = Quaternion.identity;
        GameObject instance = Object.Instantiate(worldObject);
        instance.transform.SetParent(structureInstance.transform);
        instance.SetActive(true);
        structureInstance.SetActive(true);
        structureInstance.layer = layerMask;
        structureInstance.name = DisplayName;
        NormalizeObject(instance);
        SetColliderLayer(instance, structureInstance);

        return structureInstance;
    }

    // -------------------- Private Methods --------------------

    private async Task LoadWorldObject()
    {
        GameObject loadedObject = new($"Loaded_{DisplayName}");
        await API.GltfHandler.Load(loadedObject, objectUrl);
        isObjectLoaded = true;
        worldObject = loadedObject.transform.GetChild(0).gameObject;
        worldObject.SetActive(false);
    }

    private void NormalizeObject(GameObject instance)
    {
        instance.transform.localScale = size;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.rotation = Quaternion.Euler(rotation);
    }

    private void SetColliderLayer(GameObject instance, GameObject structureInstance)
    {
        BoxCollider instanceCollider = instance.AddComponent<BoxCollider>();
        BoxCollider structureCollider = structureInstance.AddComponent<BoxCollider>();
        structureCollider.size = instanceCollider.size;
        structureCollider.center = instanceCollider.center;
        instanceCollider.enabled = false;
    }
}
