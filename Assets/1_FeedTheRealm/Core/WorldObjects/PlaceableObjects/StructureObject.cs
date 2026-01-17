using System.Threading.Tasks;
using Models;
using UnityEngine;

[System.Serializable]
public class StructureObject : IPlaceable
{
    public StructureData data;
    public string structureFilepath;
    private bool isObjectLoaded = false;
    public GameObject structurePrefab;
    private GameObject worldObject;

    public StructureObject(StructureData data, string structureFilepath, GameObject structurePrefab)
    {
        this.data = data;
        this.structureFilepath = structureFilepath;
        this.structurePrefab = structurePrefab;
    }

    public string DisplayName => data.structureName.Replace("_", " ").Replace("-", " ").ToUpper();

    public async Task<GameObject> GetPlaceableObject(int layerMask)
    {
        if (!isObjectLoaded)
            await LoadWorldObject();
        GameObject structureInstance = CreateStructureInstance();
        GameObject childInstance = CreateChildInstance(structureInstance);
        ConfigureGameObjectProperties(structureInstance, layerMask);
        SetupObjectTransforms(childInstance);
        SetupColliders(childInstance, structureInstance);

        return structureInstance;
    }

    // -------------------- Private Methods --------------------

    private GameObject CreateStructureInstance()
    {
        GameObject structureInstance = Object.Instantiate(structurePrefab);
        StructureController structureController =
            structureInstance.GetComponent<StructureController>();
        structureController.structureData = data;
        return structureInstance;
    }

    private GameObject CreateChildInstance(GameObject parent)
    {
        GameObject instance = Object.Instantiate(worldObject);
        instance.transform.SetParent(parent.transform);
        instance.SetActive(true);
        return instance;
    }

    private void ConfigureGameObjectProperties(GameObject structureInstance, int layerMask)
    {
        structureInstance.transform.position = Vector3.zero;
        structureInstance.transform.rotation = Quaternion.identity;
        structureInstance.SetActive(true);
        structureInstance.layer = layerMask;
        structureInstance.name = DisplayName;
    }

    private void SetupObjectTransforms(GameObject childInstance)
    {
        NormalizeObject(childInstance);
    }

    private void SetupColliders(GameObject childInstance, GameObject structureInstance)
    {
        SetColliderLayer(childInstance, structureInstance);
    }

    private async Task LoadWorldObject()
    {
        worldObject = new($"Loaded_{DisplayName}");
        await API.GltfHandler.Load(worldObject, structureFilepath);
        isObjectLoaded = true;
        worldObject.SetActive(false);
        NormalizeObject(worldObject);
    }

    private void NormalizeObject(GameObject gameObject)
    {
        gameObject.transform.localScale = data.size;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.rotation = Quaternion.Euler(data.rotation);
        gameObject.transform.localPosition = data.offset;
        foreach (Transform child in gameObject.transform)
        {
            NormalizeObject(child.gameObject);
        }
    }

    private void SetColliderLayer(GameObject instance, GameObject structureInstance)
    {
        Bounds bounds = GetBounds(instance);
        BoxCollider instanceCollider = instance.AddComponent<BoxCollider>();
        instanceCollider.size = bounds.size;
        instanceCollider.center = bounds.center;
        instanceCollider.enabled = false;
        BoxCollider structureCollider = structureInstance.AddComponent<BoxCollider>();
        structureCollider.size = bounds.size;
        structureCollider.center = bounds.center;
        AddCollidersToChildren(instance);
    }

    private Bounds GetBounds(GameObject gameObject)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }
        return new Bounds(gameObject.transform.position, Vector3.one);
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
            AddCollidersToChildren(child.gameObject);
        }
    }
}
