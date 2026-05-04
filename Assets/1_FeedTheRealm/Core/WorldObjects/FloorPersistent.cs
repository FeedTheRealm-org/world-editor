using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FTRShared.Runtime.Models;
using UnityEngine;
using VContainer;

namespace FeedTheRealm.Core.WorldObjects
{
    // [RequireComponent(typeof(WorldControllerV2))]
    public class FloorPersistent : MonoBehaviour, IPersistent<ZoneData>
    {
        [Inject]
        protected ZoneDataRegistryEvent registryEvent;

        private WorldControllerV2 worldController;

        private void Awake()
        {
            worldController = FindFirstObjectByType<WorldControllerV2>();
            Debug.Log(
                $"[FloorPersistent] Awake called. WorldControllerV2 found: {worldController != null}"
            );
        }

        private void Start()
        {
            if (registryEvent != null)
            {
                registryEvent.Raise(this);
            }
        }

        public void SaveData(ref ZoneData zoneData)
        {
            if (worldController != null)
            {
                zoneData.floorMaterialId = worldController.CurrentMaterialId;
                zoneData.textureGranularity = worldController.CurrentGranularity;
            }
        }
    }
}
