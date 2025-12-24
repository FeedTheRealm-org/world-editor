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
    /// Loads a GLTF model asynchronously and returns it without instantiating in the scene.
    /// </summary>
    public static async Task<GameObject> Load(string modelUrl, bool useFileProtocol = true)
    {
        if (string.IsNullOrEmpty(modelUrl))
        {
            return CreateFallback();
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
            return CreateFallback();
        }

        // Create a temporary parent, instantiate into it, then detach
        GameObject tempParent = new GameObject("_TempParent");
        await gltf.InstantiateMainSceneAsync(tempParent.transform);

        GameObject loadedObject = tempParent.transform.GetChild(0).gameObject;
        loadedObject.transform.SetParent(null); // Detach from temp parent

        Object.Destroy(tempParent); // Clean up temp parent

        // Reset transforms
        loadedObject.transform.localScale = Vector3.one;
        loadedObject.transform.localPosition = Vector3.zero;
        loadedObject.transform.localRotation = Quaternion.identity;

        return loadedObject;
    }

    /// <summary>
    /// Creates a fallback object without instantiating in the scene.
    /// </summary>
    private static GameObject CreateFallback()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = FALLBACK_OBJECT_NAME;
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = Vector3.one;
        return cube;
    }
}
