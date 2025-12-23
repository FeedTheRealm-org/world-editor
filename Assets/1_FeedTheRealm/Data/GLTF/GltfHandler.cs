using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

public static class GltfHandler
{
    private const string MODELS_FOLDER = "Models";
    private const string GLTF_EXTENSION = ".glb";
    private const string FILE_PROTOCOL = "file://";
    private const string FALLBACK_OBJECT_NAME = "MissingObject";

    /// <summary>
    /// Loads a GLTF model asynchronously from the specified path.
    /// </summary>
    public static async Task Load(
        string modelUrl,
        GameObject parentRef,
        bool useFileProtocol = true
    )
    {
        if (string.IsNullOrEmpty(modelUrl))
        {
            SpawnFallback(parentRef.transform);
            return;
        }

        if (useFileProtocol)
        {
            modelUrl =
                FILE_PROTOCOL
                + Path.Combine(
                    Application.streamingAssetsPath,
                    MODELS_FOLDER,
                    modelUrl + GLTF_EXTENSION
                );
        }

        var gltf = new GltfImport();
        bool success = await gltf.Load(modelUrl);

        if (!success)
        {
            Debug.LogWarning($"GLTF load failed: {modelUrl}");
            SpawnFallback(parentRef.transform);
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
    private static void SpawnFallback(Transform parent)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(parent);
        cube.name = FALLBACK_OBJECT_NAME;
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = Vector3.one;
    }
}
