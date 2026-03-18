using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Repository;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Core.DataPersistence
{
    public class DataPersistenceManager
    {
        private Logging.Logger logger;
        private WorldsRepository worldsRepository;
        private WorldSelector worldSelector;
        private List<IPersistent> registeredEntities = new();

        // ---------------- Public Methods ----------------
        public DataPersistenceManager(
            Logging.Logger logger,
            WorldsRepository worldsRepository,
            WorldSelector worldSelector,
            DataPersistenceRegistryEvent registryEvent
        )
        {
            this.logger = logger;
            this.worldsRepository = worldsRepository;
            this.worldSelector = worldSelector;
            registryEvent.OnRaised += RegisterEntity;
        }

        public void SaveWorld()
        {
            WorldData worldData = LoadRequiredWorldData();
            foreach (var obj in registeredEntities)
            {
                obj.SaveData(ref worldData);
            }
            worldsRepository.SaveWorldZone(worldData);
        }

        private void RegisterEntity(IPersistent entity)
        {
            if (entity == null)
            {
                logger.Log("Attempted to register a null entity.", Logging.LogType.Warning);
                return;
            }
            registeredEntities.Add(entity);
            logger.Log($"Registered entity: {entity.GetType().Name}");
        }

        /// <summary>
        ///  Loads only the essential world data (world name, zone id, and world id) required
        ///  for WorldObjects to save their data. This avoids loading unnecessary data for entities that only need to save.
        ///  Sadly, since the world ID is in the world data, we have to load it to get the ID for new zones.
        ///  If we had a separate metadata file for each zone, we could avoid this.
        /// <returns></returns>
        private WorldData LoadRequiredWorldData()
        {
            string worldId =
                worldsRepository
                    .GetWorldZone(worldSelector.selectedWorld, worldSelector.selectedZoneId)
                    ?.id
                ?? string.Empty;

            return new WorldData
            {
                id = worldId,
                zone_id = worldSelector.selectedZoneId,
                worldName = worldSelector.selectedWorld,
            };
        }
    }
}
