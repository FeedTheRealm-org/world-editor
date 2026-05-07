using System.Collections.Generic;
using System.Linq;
using FeedTheRealm.Core.DataPersistence;
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
            SetupZoneArea(worldPrefabProvider.zoneAreaPrefab);
        }

        public void SaveData(ref ZoneData data)
        {
            data.zoneAreaData = ZoneController.Data;
        }

        public void LoadData(ZoneData data)
        {
            LoadGroundTexture(data);
            LoadSkybox(data);
            registryEvent.Raise(this);
        }

        private void SetupZoneArea(GameObject zonePrefab)
        {
            var zoneInstance = Object.Instantiate(zonePrefab);
            zoneInstance.name = "Zone";
            zoneInstance.layer = Mathf.RoundToInt(Mathf.Log(config.PlaceableLayerMask.value, 2));
            zoneArea = zoneInstance;
        }

        private void AssignDefaultMaterial()
        {
            var defaultMaterial = zoneMaterialsRepository.GetMaterial(
                config.defaultMaterialId,
                ZoneTextureType.Ground
            );
            if (defaultMaterial == null)
            {
                logger.Log(
                    $"[ZoneManager] Default material '{config.defaultMaterialId}' not found in repository.",
                    Logging.LogType.Error
                );
                return;
            }
            ZoneController.ChangeMaterial(defaultMaterial, config.defaultMaterialId);
        }

        private void LoadGroundTexture(ZoneData data)
        {
            var material = zoneMaterialsRepository.GetMaterial(
                data.zoneAreaData.zoneMaterialId,
                ZoneTextureType.Ground
            );
            if (string.IsNullOrEmpty(data.zoneAreaData.zoneMaterialId) || material == null)
            {
                logger.Log(
                    "[ZoneManager] No zone material was missing or zone data was not found, loading defaults.",
                    Logging.LogType.Warning
                );
                AssignDefaultMaterial();
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
            ZoneController.SetSkyboxMaterial(material, data.zoneAreaData.skyboxMaterialId);
        }
    }
}
