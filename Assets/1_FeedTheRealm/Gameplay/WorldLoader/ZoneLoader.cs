using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects;
using FTR.Core.Loaders;
using FTRShared.Runtime.Models;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldLoader
{
    /// <summary>
    /// Orchestrates the loading of all placeable objects in a zone.
    ///
    /// Unlike CreatablesLoader which loads pure C# data objects with a simple foreach,
    /// placeables are Unity GameObjects that require async asset loading, prefab instantiation,
    /// and component injection — each with their own lookup logic (e.g. structures fetch by id,
    /// spawners fetch by type). Each loader encapsulates this variation, keeping this class
    /// focused purely on orchestration.
    /// </summary>
    public class ZoneLoader
    {
        private readonly WorldSelector worldSelector;
        private readonly DataPersistenceManager dataPersistenceManager;
        private readonly Logging.Logger logger;
        private readonly List<IPlaceableLoader> zoneLoaders;
        private readonly CloseAllEvent closeAllEvent;
        private readonly ZoneManager zoneManager;

        public ZoneLoader(
            WorldSelector worldSelector,
            DataPersistenceManager dataPersistenceManager,
            Logging.Logger logger,
            PlayerSpawnpointLoader playerSpawnpointLoader,
            StructureLoader structureLoader,
            AggresiveNpcSpawnerLoader aggresiveNpcSpawnerLoader,
            FriendlyNpcSpawnerLoader friendlyNpcSpawnerLoader,
            ZoneManager zoneManager,
            PortalLoader portalLoader,
            ChestLoader chestLoader,
            CloseAllEvent closeAllEvent
        )
        {
            this.worldSelector = worldSelector;
            this.dataPersistenceManager = dataPersistenceManager;
            this.logger = logger;
            this.zoneManager = zoneManager;
            this.closeAllEvent = closeAllEvent;

            zoneLoaders = new List<IPlaceableLoader>
            {
                playerSpawnpointLoader,
                structureLoader,
                aggresiveNpcSpawnerLoader,
                friendlyNpcSpawnerLoader,
                portalLoader,
                chestLoader,
            };
        }

        public async UniTask Load()
        {
            closeAllEvent.Raise(); // Ensure any open menus are closed before loading a new zone
            dataPersistenceManager.ClearPlaceables();
            zoneManager.RegisterZone();

            ZoneData zoneData = dataPersistenceManager.GetZoneData(
                worldSelector.selectedWorld,
                worldSelector.selectedZoneId
            );

            // we validate if zoneData is null here, do not move
            zoneManager.LoadData(zoneData);

            if (zoneData == null)
            {
                logger.Log("[ZoneLoader] No zone data found, skipping load.");
                return;
            }

            foreach (var loader in zoneLoaders)
            {
                try
                {
                    await loader.Load(zoneData);
                    logger.Log($"[ZoneLoader] {loader.GetType().Name} completed.");
                }
                catch (System.Exception ex)
                {
                    logger.Log($"[ZoneLoader] Error in {loader.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
