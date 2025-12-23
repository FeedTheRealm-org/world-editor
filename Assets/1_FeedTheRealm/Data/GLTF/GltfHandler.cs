using GLTFast;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public static class GltfHandler {

    private const string MODELS_FOLDER = "Models";
    private const string GLTF_EXTENSION = ".glb";
    private const string FILE_PROTOCOL = "file://";

    /// <summary>
    /// Loads a GLTF model asynchronously from the specified path.
    /// </summary>
    public static async Task Load(
        string modelUrl,
        GameObject parentRef,
        GameObject fallbackPrefab = null,
        bool useFileProtocol = true
    ) {
        if (string.IsNullOrEmpty(modelUrl)) {
            SpawnFallback(parentRef.transform, fallbackPrefab);
            return;
        }

        if (useFileProtocol) {
            modelUrl = FILE_PROTOCOL + Path.Combine(
            Application.streamingAssetsPath,
            MODELS_FOLDER,
            modelUrl + GLTF_EXTENSION);
        }

        var gltf = new GltfImport();
        bool success = await gltf.Load(modelUrl);

        if (!success) {
            Debug.LogWarning($"GLTF load failed: {modelUrl}");
            SpawnFallback(parentRef.transform, fallbackPrefab);
            return;
        }

        await gltf.InstantiateMainSceneAsync(parentRef.transform);
        var child = parentRef.transform.GetChild(0);
        child.localScale = Vector3.one;
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Spawns a fallback prefab if the GLTF load fails.
    /// </summary>
    private static void SpawnFallback(
        Transform parent,
        GameObject fallbackPrefab
    ) {
        if (fallbackPrefab == null) return;

        var go = Object.Instantiate(fallbackPrefab, parent);
        go.name = "MissingObject";
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }
}
