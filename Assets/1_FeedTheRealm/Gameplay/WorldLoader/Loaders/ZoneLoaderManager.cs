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

        private List<ILoader> loaders;

        public ZoneLoaderManager(
            WorldSelector worldSelector,
            DataPersistenceManager dataPersistenceManager,
            Logging.Logger logger,
            IObjectResolver resolver
        )
        {
            this.worldSelector = worldSelector;
            this.dataPersistenceManager = dataPersistenceManager;
            this.logger = logger;

            loaders = new List<ILoader>()
            {
                resolver.Resolve<PlayerSpawnpointLoader>(),
                resolver.Resolve<StructureLoader>(),
                resolver.Resolve<AggresiveNpcSpawnerLoader>(),
                resolver.Resolve<FriendlyNpcSpawnerLoader>(),
            };
        }

        public async UniTask Load()
        {
            ZoneData zoneData = dataPersistenceManager.GetZoneData(
                worldSelector.selectedWorld,
                worldSelector.selectedZoneId
            );
            if (zoneData == null)
                return;
            for (int i = 0; i < loaders.Count; i++)
            {
                try
                {
                    await loaders[i].Load(zoneData);
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
    }
}
