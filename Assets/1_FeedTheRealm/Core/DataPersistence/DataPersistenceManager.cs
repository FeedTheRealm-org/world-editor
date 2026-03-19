using System;
using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Repository;
using FTRShared.Runtime.Models;
using VContainer.Unity;

namespace FeedTheRealm.Core.DataPersistence
{
    public class DataPersistenceManager : IInitializable
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

        // IInitializable requiers this method, but we don't need to do anything on initialization for this repository
        // We implement this interface just to ensure that the repository is created when registered.
        public void Initialize() { }

        public void SaveWorld(string worldName, int zoneId = -1)
        {
            if (
                worldSelector.selectedWorld == null
                || string.IsNullOrEmpty(worldSelector.selectedWorld)
            )
            {
                logger.Log("No world selected for saving.", Logging.LogType.Warning);
                return;
            }
            WorldData worldData = LoadRequiredWorldData(worldName, zoneId);
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
        private WorldData LoadRequiredWorldData(string worldName, int zoneId)
        {
            // TODO: change this to move the world name and id to another file

            string worldId = worldsRepository.GetWorldZone(worldName, zoneId)?.id ?? string.Empty;

            return new WorldData
            {
                id = worldId,
                zone_id = zoneId,
                worldName = worldName,
            };
        }
    }
}
