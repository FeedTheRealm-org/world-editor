using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.Repository;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using VContainer;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    public class WorldLoaderManager
    {
        private readonly WorldSelector worldSelector;
        private readonly WorldsRepository worldsRepository;
        private readonly Logging.Logger logger;

        private List<ILoader> loaders;

        public WorldLoaderManager(
            WorldSelector worldSelector,
            WorldsRepository worldsRepository,
            Logging.Logger logger,
            IObjectResolver resolver
        )
        {
            this.worldSelector = worldSelector;
            this.worldsRepository = worldsRepository;
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
            WorldData worldData = worldsRepository.GetWorldZone(
                worldSelector.selectedWorld,
                worldSelector.selectedZoneId
            );
            if (worldData == null)
                return;
            for (int i = 0; i < loaders.Count; i++)
            {
                try
                {
                    await loaders[i].Load(worldData);
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
