using System.Collections.Generic;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Loaders;
using FTR.Core.Common.Loaders;
using FTRShared.Runtime.Models;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldLoaderManager
    {
        private readonly DataPersistenceManagerSO dataPersistenceManager;
        private readonly Logging.Logger logger;
        private readonly IObjectResolver objectResolver;
        private readonly WorldPrefabProvider worldPrefabProvider;

        public WorldLoaderManager(
            DataPersistenceManagerSO dataPersistenceManager,
            IObjectResolver objectResolver,
            Logging.Logger logger,
            WorldPrefabProvider worldPrefabProvider
        )
        {
            this.dataPersistenceManager = dataPersistenceManager;
            this.objectResolver = objectResolver;
            this.logger = logger;
            this.worldPrefabProvider = worldPrefabProvider;
        }

        public void Load()
        {
            WorldData worldData = dataPersistenceManager.CurrentWorldData;
            var loaders = GetLoaders();
            for (int i = 0; i < loaders.Count; i++)
            {
                try
                {
                    objectResolver.Inject(loaders[i]);
                    loaders[i].Load(worldData, worldPrefabProvider, objectResolver);
                    logger.Log(
                        $"Loader {i} / {loaders.Count} | {loaders[i].GetType().Name} completed loading."
                    );
                }
                catch (System.Exception ex)
                {
                    logger.Log($"Error loading {loaders[i].GetType().Name}: {ex.Message}");
                }
            }
        }

        private List<ILoader> GetLoaders()
        {
            var structureLoader = new StructureLoader();
            var friendlyNpcSpawnerLoader = new FriendlyNpcSpawnerLoader();
            var aggresiveNpcSpawnerLoader = new AggresiveNpcSpawnerLoader();
            var playerSpawnPointLoader = new PlayerSpawnpointLoader();
            return new List<ILoader>()
            {
                structureLoader,
                friendlyNpcSpawnerLoader,
                aggresiveNpcSpawnerLoader,
                playerSpawnPointLoader,
            };
        }
    }
}
