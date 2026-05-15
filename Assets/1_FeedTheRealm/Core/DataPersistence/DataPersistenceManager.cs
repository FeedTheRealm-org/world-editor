using System;
using System.Collections.Generic;
using System.Linq;
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
        private ModelsRepository modelsRepository;
        private CreatablesRepository creatablesRepository;
        private List<IPersistent<ZoneData>> registeredPlaceables = new();
        private List<IPersistent<CreatablesData>> registeredCreatables = new();

        // ---------------- Public Methods ----------------
        public DataPersistenceManager(
            Logging.Logger logger,
            WorldsRepository worldsRepository,
            ZonesRepository zonesRepository,
            CreatablesRepository creatablesRepository,
            ModelsRepository modelsRepository,
            ZoneDataRegistryEvent registryEvent,
            CreatablesDataRegistryEvent creatablesRegistryEvent
        )
        {
            this.logger = logger;
            this.worldsRepository = worldsRepository;
            this.zonesRepository = zonesRepository;
            this.creatablesRepository = creatablesRepository;
            this.modelsRepository = modelsRepository;
            registryEvent.OnRaised += RegisterEntity;
            creatablesRegistryEvent.OnRaised += RegisterEntity;
        }

        // IInitializable requiers this method, but we don't need to do anything on initialization for this repository
        // We implement this interface just to ensure that the repository is created when registered.
        public void Initialize() { }

        // --- Create/Save Methods ---

        public WorldData CreateNewWorld(string worldName)
        {
            return new WorldData { worldName = worldName, created_at = DateTime.Now };
        }

        public void SaveWorldMetadata(WorldData worldData)
        {
            worldsRepository.SaveWorldData(worldData);
        }

        public void SaveZone(string worldName, int zoneId)
        {
            var zoneData = new ZoneData(worldName, zoneId);

            logger.Log(
                $"[DataPersistenceManager] Saving zone data for world: {worldName}, zone: {zoneId} | Registered placeables count: {registeredPlaceables.Count}"
            );

            registeredPlaceables.RemoveAll(obj =>
                obj is UnityEngine.Object unityObj && unityObj == null
            );

            // If shop is deleted, here the placeable set as shop will be
            // updated to not be a shop anymore.
            registeredCreatables.RemoveAll(obj => obj == null);
            var currentCreatables = new CreatablesData();
            foreach (var obj in registeredCreatables)
                obj.SaveData(ref currentCreatables);

            foreach (var obj in registeredPlaceables)
                obj.SaveData(ref zoneData);

            var validShopIds = new HashSet<string>(
                currentCreatables?.shops?.Select(s => s.id) ?? Enumerable.Empty<string>()
            );

            foreach (var structure in zoneData.objectPlacementData)
            {
                if (structure.isShop && !validShopIds.Contains(structure.shopId))
                {
                    structure.isShop = false;
                    structure.shopId = string.Empty;
                }
            }

            zonesRepository.SaveZoneData(worldName, zoneData);
        }

        public void SaveCreatables(string worldName)
        {
            var creatablesData = new CreatablesData();
            registeredCreatables.RemoveAll(obj => obj == null);
            foreach (var obj in registeredCreatables)
                obj.SaveData(ref creatablesData);
            creatablesRepository.SaveCreatables(worldName, creatablesData);
        }

        public void SaveCreatablesData(string worldName, CreatablesData creatablesData)
        {
            creatablesRepository.SaveCreatables(worldName, creatablesData);
        }

        // ---- Registration Methods ----

        private void RegisterEntity(IPersistent<ZoneData> entity)
        {
            logger.Log($"[DataPersistenceManager] Registering entity: {entity}");
            if (entity == null)
            {
                logger.Log(
                    "[Data Persistence Manager] Attempted to register a null entity.",
                    Logging.LogType.Warning
                );
                return;
            }
            registeredPlaceables.Add(entity);
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
        }

        // ---- Get Methods ----

        public ZoneData GetZoneData(string worldName, int zoneId)
        {
            return zonesRepository.GetZoneData(worldName, zoneId);
        }

        public CreatablesData GetCreatables(string worldName)
        {
            return creatablesRepository.GetCreatables(worldName);
        }

        public WorldData GetWorldData(string worldName)
        {
            if (string.IsNullOrEmpty(worldName))
                return null;
            return worldsRepository.GetWorldData(worldName);
        }

        public int GetNextZoneId(string worldName)
        {
            return zonesRepository.GetNextZoneId(worldName);
        }

        // ---- List Methods ----

        public List<string> ListAllWorlds()
        {
            return worldsRepository.ListWorlds();
        }

        public List<int> ListZones(string worldName)
        {
            if (string.IsNullOrEmpty(worldName))
                return new List<int>();
            return zonesRepository.ListZones(worldName);
        }

        // ----- Data Management -----
        /// <summary>
        /// Clears the registry of placeables and creatables.
        /// This is typically called before loading a new world to ensure
        /// that data from the previous world does not persist.
        /// </summary>
        public void ClearPlaceables()
        {
            foreach (var obj in registeredPlaceables)
            {
                var component = obj as UnityEngine.Component;
                if (component != null)
                    UnityEngine.Object.Destroy(component.gameObject);
            }
            registeredPlaceables.RemoveAll(obj => obj as UnityEngine.Component != null);
            logger.Log("[DataPersistenceManager] Registry cleared.");
        }

        public void ClearCreatables()
        {
            registeredCreatables.RemoveAll(obj => obj == null);
            registeredCreatables.Clear();
            logger.Log("[DataPersistenceManager] Creatables registry cleared.");
        }

        public string GetModelFilepath(string modelId)
        {
            return modelsRepository.GetModelFilepath(modelId);
        }

        public string GetCurrentWorldId(string selectedWorld)
        {
            var worldData = GetWorldData(selectedWorld);
            return worldData != null ? worldData.worldId : null;
        }
    }
}
