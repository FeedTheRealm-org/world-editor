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
        worldObject = new($"Loaded_{DisplayName}");
        await API.GltfHandler.Load(worldObject, objectUrl);
        isObjectLoaded = true;
        worldObject.SetActive(false);
        NormalizeObject(worldObject);
    }

    private void NormalizeObject(GameObject gameObject)
    {
        gameObject.transform.localScale = size;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.rotation = Quaternion.Euler(rotation);
        gameObject.transform.localPosition = offset;
        foreach (Transform child in gameObject.transform)
        {
            NormalizeObject(child.gameObject);
        }
    }

    private void SetColliderLayer(GameObject instance, GameObject structureInstance)
    {
        // Calculate bounds from all child renderers
        Bounds bounds = GetBounds(instance);

        // Add collider to instance with calculated bounds
        BoxCollider instanceCollider = instance.AddComponent<BoxCollider>();
        instanceCollider.size = bounds.size;
        instanceCollider.center = bounds.center;
        instanceCollider.enabled = false;

        // Add collider to structure with the same bounds
        BoxCollider structureCollider = structureInstance.AddComponent<BoxCollider>();
        structureCollider.size = bounds.size;
        structureCollider.center = bounds.center;

        // Add colliders to all children as well
        AddCollidersToChildren(instance);
    }

    private Bounds GetBounds(GameObject gameObject)
    {
        Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        else
        {
            // Fallback to transform bounds if no renderers
            bounds = new Bounds(gameObject.transform.position, Vector3.one);
        }

        return bounds;
    }

    private void AddCollidersToChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null && child.GetComponent<BoxCollider>() == null)
            {
                BoxCollider collider = child.gameObject.AddComponent<BoxCollider>();
                collider.size = renderer.bounds.size;
                collider.center = renderer.bounds.center - child.position;
            }

            // Recursively add colliders to grandchildren
            AddCollidersToChildren(child.gameObject);
        }
    }
}
