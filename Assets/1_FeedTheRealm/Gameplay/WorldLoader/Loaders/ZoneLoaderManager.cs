using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.DataPersistence;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class ZoneLoaderManager
    {
        private readonly WorldSelector worldSelector;
        private readonly DataPersistenceManager dataPersistenceManager;
        private readonly Logging.Logger logger;
        private readonly List<IPlaceableLoader> zoneLoaders;
        private readonly CreatableLoader creatablesLoader;

        public ZoneLoaderManager(
            WorldSelector worldSelector,
            DataPersistenceManager dataPersistenceManager,
            Logging.Logger logger,
            CreatableLoader creatablesLoader,
            IObjectResolver resolver
        )
        {
            this.worldSelector = worldSelector;
            this.dataPersistenceManager = dataPersistenceManager;
            this.logger = logger;

            zoneLoaders = new List<IPlaceableLoader>()
            {
                resolver.Resolve<PlayerSpawnpointLoader>(),
                resolver.Resolve<StructureLoader>(),
                resolver.Resolve<AggresiveNpcSpawnerLoader>(),
                resolver.Resolve<FriendlyNpcSpawnerLoader>(),
            };

            this.creatablesLoader = creatablesLoader;
        }

        public async UniTask Load()
        {
            await LoadCreatables();
            await LoadPlaceables();
        }

        private async UniTask LoadCreatables()
        {
            try
            {
                CreatablesData creatablesData = dataPersistenceManager.GetCreatables(
                    worldSelector.selectedWorld
                );

                await creatablesLoader.Load(worldSelector.selectedWorld, creatablesData);
                logger.Log($"CreatableLoader completed loading.");
            }
            catch (System.Exception ex)
            {
                logger.Log($"Error loading creatables: {ex.Message}");
            }
        }

        private async UniTask LoadPlaceables()
        {
            ZoneData zoneData = dataPersistenceManager.GetZoneData(
                worldSelector.selectedWorld,
                worldSelector.selectedZoneId
            );
            if (zoneData == null)
                return;
            for (int i = 0; i < zoneLoaders.Count; i++)
            {
                try
                {
                    await zoneLoaders[i].Load(zoneData);
                    logger.Log(
                        $"Loader {i} / {zoneLoaders.Count} | {zoneLoaders[i].GetType().Name} completed loading."
                    );
                }
                catch (System.Exception ex)
                {
                    logger.Log($"Error loading {zoneLoaders[i].GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
