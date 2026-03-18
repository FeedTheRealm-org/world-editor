using System.Collections.Generic;
using API;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary
{
    public class StructureLibrary : ILibrary
    {
        private GltfService gltfService;
        private ModelsRepository modelsRepository;
        private GameObject structurePrefab;
        private Logging.Logger logger;
        private Dictionary<string, GameObject> structuresCache;
        private IObjectResolver resolver;

        public StructureLibrary(
            GltfService gltfService,
            ModelsRepository modelsRepository,
            WorldPrefabProvider prefabProvider,
            Logging.Logger logger,
            IObjectResolver resolver
        )
        {
            this.gltfService = gltfService;
            this.modelsRepository = modelsRepository;
            this.logger = logger;
            this.resolver = resolver;
            structurePrefab = prefabProvider.structurePrefab;
            structuresCache = new Dictionary<string, GameObject>();
        }

        public async UniTask<GameObject> GetItem(string structureId)
        {
            return await GetStructureInstance(structureId);
        }

        public List<string> ListAvailableItems()
        {
            return new List<string>(modelsRepository.ListAvailableModels());
        }

        private async UniTask<GameObject> GetStructureInstance(string structureId)
        {
            if (!structuresCache.TryGetValue(structureId, out var cachedStructure))
            {
                var modelData = modelsRepository.GetModelData(structureId);
                if (modelData == null)
                {
                    logger.Log(
                        $"Model data for {structureId} not found in ModelsRepository.",
                        Logging.LogType.Error
                    );
                    return null;
                }
                await LoadStructureFromDisk(modelData);
                cachedStructure = structuresCache[structureId];
            }
            var instance = resolver.Instantiate(cachedStructure);
            instance.SetActive(true);
            return instance;
        }

        private async UniTask LoadStructureFromDisk(ModelData modeldata)
        {
            GameObject structureInstance = resolver.Instantiate(structurePrefab);
            await gltfService.Load(structureInstance, modeldata.filePath);
            structureInstance.name = modeldata.id;
            structureInstance.SetActive(false);
            structuresCache[modeldata.id] = structureInstance;
        }
    }
}
