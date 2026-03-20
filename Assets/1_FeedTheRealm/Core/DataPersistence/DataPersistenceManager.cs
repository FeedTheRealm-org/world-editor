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
        private List<IPersistent> registeredEntities = new();

        // ---------------- Public Methods ----------------
        public DataPersistenceManager(
            Logging.Logger logger,
            WorldsRepository worldsRepository,
            DataPersistenceRegistryEvent registryEvent
        )
        {
            this.logger = logger;
            this.worldsRepository = worldsRepository;
            registryEvent.OnRaised += RegisterEntity;
        }

        // IInitializable requiers this method, but we don't need to do anything on initialization for this repository
        // We implement this interface just to ensure that the repository is created when registered.
        public void Initialize() { }

        public void SaveWorld(string worldName, int zoneId = -1)
        {
            // ZoneData zoneData = LoadRequiredWorldData(worldName, zoneId);
            // foreach (var obj in registeredEntities)
            // {
            //     obj.SaveData(ref zoneData);
            // }
            // worldsRepository.SaveWorldZone(zoneData);
        }

        private void RegisterEntity(IPersistent entity)
        {
            if (entity == null)
            {
                logger.Log(
                    "[Data Persistence Manager] Attempted to register a null entity.",
                    Logging.LogType.Warning
                );
                return;
            }
            registeredEntities.Add(entity);
            logger.Log($"[Data Persistence Manager] Registered entity: {entity.GetType().Name}");
        }

        /// <summary>
        ///  Loads only the essential world data (world name, zone id, and world id) required
        ///  for WorldObjects to save their data. This avoids loading unnecessary data for entities that only need to save.
        ///  Sadly, since the world ID is in the world data, we have to load it to get the ID for new zones.
        ///  If we had a separate metadata file for each zone, we could avoid this.
        /// </summary>
        private ZoneData GetZoneData(string worldName, int zoneId)
        {
            // string worldId = worldsRepository.GetWorldData(worldName, zoneId)?.id ?? string.Empty;

            return new ZoneData { };
        }
    }
}
