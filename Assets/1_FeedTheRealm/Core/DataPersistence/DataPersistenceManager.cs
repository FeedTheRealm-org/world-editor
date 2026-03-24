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
        private ZonesRepository zonesRepository;
        private CreatablesRepository creatablesRepository;
        private List<IPersistent<ZoneData>> registeredPlaceables = new();
        private List<IPersistent<CreatablesData>> registeredCreatables = new();

        // ---------------- Public Methods ----------------
        public DataPersistenceManager(
            Logging.Logger logger,
            WorldsRepository worldsRepository,
            ZonesRepository zonesRepository,
            CreatablesRepository creatablesRepository,
            ZoneDataRegistryEvent registryEvent,
            CreatablesDataRegistryEvent creatablesRegistryEvent
        )
        {
            this.logger = logger;
            this.worldsRepository = worldsRepository;
            this.zonesRepository = zonesRepository;
            this.creatablesRepository = creatablesRepository;
            registryEvent.OnRaised += RegisterEntity;
            creatablesRegistryEvent.OnRaised += RegisterEntity;
        }

        // IInitializable requiers this method, but we don't need to do anything on initialization for this repository
        // We implement this interface just to ensure that the repository is created when registered.
        public void Initialize() { }

        // --- Create/Save Methods ---

        public WorldData CreateNewWorld(string worldName)
        {
            return new WorldData
            {
                worldId = Guid.NewGuid().ToString(),
                worldName = worldName,
                created_at = DateTime.Now,
                last_edited_at = DateTime.Now,
            };
        }

        public void SaveWorldMetadata(WorldData worldData)
        {
            worldsRepository.SaveWorldData(worldData);
        }

        public void SaveZone(string worldId, int zoneId)
        {
            var zoneData = new ZoneData(worldId, zoneId);
            foreach (var obj in registeredPlaceables)
                obj.SaveData(ref zoneData);

            zonesRepository.SaveZoneData(worldId, zoneData);
        }

        public void SaveCreatables(string worldId, int zoneId)
        {
            var creatablesData = new CreatablesData();
            foreach (var obj in registeredCreatables)
                obj.SaveData(ref creatablesData);

            creatablesRepository.SaveCreatables(worldId, creatablesData);
        }

        // ---- Registration Methods ----

        private void RegisterEntity(IPersistent<ZoneData> entity)
        {
            if (entity == null)
            {
                logger.Log(
                    "[Data Persistence Manager] Attempted to register a null entity.",
                    Logging.LogType.Warning
                );
                return;
            }
            registeredPlaceables.Add(entity);
            logger.Log($"[Data Persistence Manager] Registered entity: {entity.GetType().Name}");
        }

        private void RegisterEntity(IPersistent<CreatablesData> entity)
        {
            if (entity == null)
            {
                logger.Log(
                    "[Data Persistence Manager] Attempted to register a null entity.",
                    Logging.LogType.Warning
                );
                return;
            }
            registeredCreatables.Add(entity);
            logger.Log($"[Data Persistence Manager] Registered entity: {entity.GetType().Name}");
        }

        // ---- Get Methods ----

        public ZoneData GetZoneData(string worldId, int zoneId)
        {
            return zonesRepository.GetZoneData(worldId, zoneId);
        }

        public CreatablesData GetCreatables(string worldId)
        {
            return creatablesRepository.GetCreatables(worldId);
        }

        public WorldData GetWorldData(string worldId)
        {
            return worldsRepository.GetWorldData(worldId);
        }

        public List<string> GetAllWorlds()
        {
            return worldsRepository.ListWorlds();
        }
    }
}
