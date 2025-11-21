using System;
using UnityEngine;
/// <summary>
/// This is the Asset Data type used to store Assets in the Database
/// Assets are considered all kind of placeable objects in the world editor
/// </summary>
[Serializable]
public class AssetData {
    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private string modelPath;
    [SerializeField] private string materialPath;
    [NonSerialized] private GameObject assetModel = null;
    [NonSerialized] private bool isModelLoaded = false;


    private void LoadModel() {
        GameObject model = Resources.Load<GameObject>(modelPath);
        GameObject material = Resources.Load<GameObject>(materialPath);
        if (model == null) {
            Debug.LogError($"Asset {name} | Model prefab not found at path: {modelPath}");
            isModelLoaded = true;
            return;
        }
        if (material != null) {
            Debug.Log($"Material loaded successfully for: {name}");
        } else {
            Debug.LogWarning($"Asset {name} | Material not found at path: {materialPath}");
        }

        assetModel = model;
        isModelLoaded = true;
    }

    private GameObject GetPrefab() {
        if (!isModelLoaded) {
            LoadModel();
        }
        return assetModel;
    }


    public GameObject InstantiateModel() {
        GameObject prefab = GetPrefab();
        if (prefab == null) {
            Debug.LogError($"Cannot instantiate - prefab is null for asset: {name}");
            return null;
        }
        GameObject instance = UnityEngine.Object.Instantiate(prefab);
        Material material = Resources.Load<Material>(materialPath);
        if (material != null && instance.TryGetComponent<Renderer>(out var renderer)) {
            renderer.material = material;
        }
        return instance;
    }

    public int Id => id;
    public string Name => name;
    public Vector2Int Size => size;
    public string ModelPath => modelPath;
    public string MaterialPath => materialPath;
    public GameObject AssetModelInstance => InstantiateModel();

}
