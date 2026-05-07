using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldObjects.Provider;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Core.WorldEditor
{
    /// <summary>
    /// This manager is in charge of modifying the physical zone's atributes like the floor material
    /// and providing the current active material data for saving.
    /// </summary>
    public class ZoneManager : IInitializable, IPersistent<ZoneData>
    {
        private ZoneMaterialsRepository zoneMaterialsRepository;
        private Config config;
        private Logging.Logger logger;
        private ZoneDataRegistryEvent registryEvent;
        private GameObject zoneArea;
        public ZoneController ZoneController => zoneArea.GetComponent<ZoneController>();

        public void Initialize() { }

        public ZoneManager(
            ZoneMaterialsRepository zoneMaterialsRepository,
            Config config,
            Logging.Logger logger,
            ZoneDataRegistryEvent registryEvent,
            WorldPrefabProvider worldPrefabProvider
        )
        {
            this.zoneMaterialsRepository = zoneMaterialsRepository;
            this.config = config;
            this.logger = logger;
            this.registryEvent = registryEvent;
            InitializeZoneObject(worldPrefabProvider.zoneAreaPrefab);
        }

        public void SaveData(ref ZoneData data)
        {
            data.zoneAreaData = ZoneController.Data;
        }

        public void RegisterZone()
        {
            registryEvent.Raise(this);
        }

        public void LoadData(ZoneData data)
        {
            if (data == null)
            {
                logger.Log("[ZoneManager] No zone data provided, loading defaults.");
                AssignDefaultMaterial(ZoneTextureType.Ground);
                AssignDefaultMaterial(ZoneTextureType.Skybox);
                return;
            }
            LoadGroundTexture(data);
            LoadSkybox(data);
            logger.Log("[ZoneManager] Zone data loaded.");
        }

        private void InitializeZoneObject(GameObject zonePrefab)
        {
            var zoneInstance = Object.Instantiate(zonePrefab);
            zoneInstance.name = "Zone";
            zoneInstance.layer = Mathf.RoundToInt(Mathf.Log(config.PlaceableLayerMask.value, 2));
            zoneArea = zoneInstance;
        }

        private void AssignDefaultMaterial(ZoneTextureType type)
        {
            logger.Log($"[ZoneManager] Assigning default {type} material.");
            var defaultMaterial = zoneMaterialsRepository.GetMaterial(
                ZoneMaterialsRepository.defaultId,
                type
            );
            if (defaultMaterial == null)
            {
                logger.Log(
                    $"[ZoneManager] Default {type} material not found.",
                    Logging.LogType.Error
                );
                return;
            }

            if (type == ZoneTextureType.Ground)
                ZoneController.ChangeMaterial(defaultMaterial, ZoneMaterialsRepository.defaultId);
            else
                ZoneController.SetSkyboxMaterial(
                    defaultMaterial,
                    ZoneMaterialsRepository.defaultId
                );
        }

        private void LoadGroundTexture(ZoneData data)
        {
            var material = zoneMaterialsRepository.GetMaterial(
                data.zoneAreaData.zoneMaterialId,
                ZoneTextureType.Ground
            );
            if (string.IsNullOrEmpty(data.zoneAreaData.zoneMaterialId) || material == null)
            {
                AssignDefaultMaterial(ZoneTextureType.Ground);
                return;
            }
            ZoneController.ChangeMaterial(material, data.zoneAreaData.zoneMaterialId);
            ZoneController.ApplyTextureGranularity(data.zoneAreaData.textureGranularity);
        }

        private void LoadSkybox(ZoneData data)
        {
            var material = zoneMaterialsRepository.GetMaterial(
                data.zoneAreaData.skyboxMaterialId,
                ZoneTextureType.Skybox
            );
            if (string.IsNullOrEmpty(data.zoneAreaData.skyboxMaterialId) || material == null)
            {
                AssignDefaultMaterial(ZoneTextureType.Skybox);
                return;
            }
            ZoneController.SetSkyboxMaterial(material, data.zoneAreaData.skyboxMaterialId);
        }
    }
}
