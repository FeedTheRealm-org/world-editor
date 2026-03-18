using System;
using Cysharp.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace API
{
    /// <summary>
    /// Service to download item sprites from API.
    /// Handles sprite downloads for items system.
    /// Route: /assets/sprites/items/{spriteId}?category={category}
    /// Separated from AssetsService (character editor sprites).
    /// </summary>
    [CreateAssetMenu(
        fileName = "GltLoaderService",
        menuName = "Scriptable Objects/API/GltLoaderService"
    )]
    public class GltfService : ScriptableObject
    {
        [SerializeField]
        private ApiConfig apiConfig;
        private const string FILE_PROTOCOL = "file://";

        public async UniTask Load(GameObject parent, string modelFile)
        {
            if (string.IsNullOrEmpty(modelFile))
            {
                CreateFallback(parent);
                return;
            }
            try
            {
                var gltfImport = new GltfImport();
                modelFile = FILE_PROTOCOL + modelFile;
                bool success = await gltfImport.Load(modelFile);
                if (!success)
                {
                    Debug.LogWarning($"Failed to load: {modelFile}");
                    CreateFallback(parent);
                    return;
                }
                await gltfImport.InstantiateMainSceneAsync(parent.transform);
                Transform child = parent.transform.GetChild(0);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"GLTF load exception for '{modelFile}': {exception.Message}");
                CreateFallback(parent);
            }
        }

        private void CreateFallback(GameObject parent)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(parent.transform);
        }
    }
}
