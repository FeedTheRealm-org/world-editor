using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

public static class GltfHandler
{
    private const string MODELS_FOLDER = "Models";
    private const string GLTF_EXTENSION = ".glb";
    private const string FILE_PROTOCOL = "file://";

    /// <summary>
    /// Loads a GLTF model into the provided parent GameObject.
    /// </summary>
    public static async Task Load(GameObject parent, string modelName, bool useLocalAsset = true)
    {
        if (string.IsNullOrEmpty(modelName))
        {
            CreateFallback(parent);
            return;
        }
        string modelPath = useLocalAsset
            ? FILE_PROTOCOL
                + Path.Combine(
                    Application.streamingAssetsPath,
                    MODELS_FOLDER,
                    modelName + GLTF_EXTENSION
                )
            : modelName;

        var gltf = new GltfImport();
        bool success = await gltf.Load(modelPath);

        if (!success)
        {
            Debug.LogWarning($"Failed to load: {modelPath}");
            CreateFallback(parent);
            return;
        }
        await gltf.InstantiateMainSceneAsync(parent.transform);
    }

    private static void CreateFallback(GameObject parent)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(parent.transform);
        cube.name = "Fallback";
    }
}
