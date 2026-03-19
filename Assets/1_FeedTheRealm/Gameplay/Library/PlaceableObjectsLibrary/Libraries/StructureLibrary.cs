using System.Collections.Generic;
using System.Linq;
using API;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.Library;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
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

        /// <summary>
        /// Returns an instance of the structure with the given id.
        /// If the structure is not cached, it loads it from disk, caches it and then returns the instance.
        /// </summary>
        public async UniTask<GameObject> GetItem(string structureId)
        {
            if (!structuresCache.TryGetValue(structureId, out var cachedStructure))
            {
                var modelData = modelsRepository.GetStructureData(structureId);
                if (modelData == null)
                {
                    logger.Log(
                        $"Model data for {structureId} not found in ModelsRepository.",
                        Logging.LogType.Error
                    );
                    return null;
                }
                await CacheStructureFromDisk(modelData);
                cachedStructure = structuresCache[structureId];
            }
            var instance = resolver.Instantiate(cachedStructure);
            instance.SetActive(true);
            return instance;
        }

        public Dictionary<string, string> ListAvailableItems()
        {
            List<StructureData> models = modelsRepository.GetModelsData().Values.ToList();
            return models.ToDictionary(model => model.id, model => model.structureName);
        }

        /// <summary>
        /// Loads the structure model from disk and caches it in memory.
        /// This is done to avoid loading the same model multiple times from disk,
        /// which can be costly.
        /// </summary>
        private async UniTask CacheStructureFromDisk(StructureData structuredata)
        {
            // we instanciate the prefab not with the resolver to avoid injecting the dependencies of the structure on the cache instance,
            // when instanciating actual structures we want the dependencies to be injected, but not when we are caching the model on disk
            GameObject cacheInstance = Object.Instantiate(structurePrefab);
            await gltfService.Load(cacheInstance, structuredata.fileName);
            cacheInstance.GetComponent<ILoadable<StructureData>>().Load(structuredata);
            cacheInstance.SetActive(false);
            structuresCache[structuredata.id] = cacheInstance;
        }
    }
}
