using System;
using UnityEngine;
/// <summary>
/// This is the Asset Data type used to store Assets in the Database
/// Assets are considered all kind of placeable objects in the world editor
/// </summary>
[Serializable]
public class Asset {
    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private string modelPath;
    [SerializeField] private string materialPath;
    [NonSerialized] private GameObject assetModel = null;
    [NonSerialized] private bool isModelLoaded = false;


    private void LoadModel() {
        try {
            GameObject model = Resources.Load<GameObject>(modelPath);
            GameObject material = Resources.Load<GameObject>(materialPath);
            if (model == null) {
                Debug.LogError($"Asset {name} | Model prefab not found at path: {modelPath}");
                return;
            }
            if (material != null) {
                Debug.Log($"Material loaded successfully for: {name}");
            } else {
                Debug.LogWarning($"Asset {name} | Material not found at path: {materialPath}");
                return;
            }

            assetModel = model;
            isModelLoaded = true;
        } catch (Exception e) {
            Debug.LogError($"Model could not be loaded for asset [{name}]: {e}");
        }
    }

    private GameObject GetPrefab() {
        if (!isModelLoaded) {
            LoadModel();
        }
        return assetModel;
    }


    public GameObject InstantiateModel() {
        try {
            GameObject prefab = GetPrefab();
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            Material material = Resources.Load<Material>(materialPath);
            if (material != null && instance.TryGetComponent<Renderer>(out var renderer)) {
                renderer.material = material;
            }
            return instance;
        } catch (Exception e) {
            Debug.LogError($"Error instantiating model for asset {name}: {e}");
            return null;
        }
    }

    public int Id => id;
    public string Name => name;
    public Vector2Int Size => size;
    public string ModelPath => modelPath;
    public string MaterialPath => materialPath;
    public GameObject AssetModelInstance => InstantiateModel();

}
