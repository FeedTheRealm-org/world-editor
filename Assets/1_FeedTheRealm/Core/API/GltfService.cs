using System;
using Cysharp.Threading.Tasks;
using FTR.Core.Common.Config;
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

        [SerializeField]
        private Config config;

        [SerializeField]
        public GameObject errorPrefab;
        private const string FILE_PROTOCOL = "file://";

        public async UniTask Load(
            GameObject parent,
            string modelFilepath,
            string LoadToObjectName = "Model"
        )
        {
            if (string.IsNullOrEmpty(modelFilepath))
            {
                CreateFallback(parent);
                return;
            }
            Transform modelContainer = parent.transform.Find(LoadToObjectName) ?? parent.transform;
            modelFilepath = $"{FILE_PROTOCOL}{config.ModelsDirectory}/{modelFilepath}";
            try
            {
                var gltfImport = new GltfImport();
                bool success = await gltfImport.Load(modelFilepath);
                if (!success)
                {
                    Debug.LogWarning($"Failed to load: {modelFilepath}");
                    CreateFallback(modelContainer.gameObject);
                    return;
                }
                await gltfImport.InstantiateMainSceneAsync(modelContainer);
                Transform child = modelContainer.GetChild(0);
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"GLTF load exception for '{modelFilepath}': {exception.Message}");
                CreateFallback(modelContainer.gameObject);
            }
        }

        private void CreateFallback(GameObject parent)
        {
            ToastNotification.Show($"Model could not be loaded", "error", Color.red);
            var cube = Instantiate(errorPrefab, parent.transform.position, Quaternion.identity);
            cube.transform.SetParent(parent.transform);
        }
    }
}
