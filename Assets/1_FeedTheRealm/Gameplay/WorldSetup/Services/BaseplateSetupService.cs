using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Core.WorldSetup;
using FTR.Core.Common.Config;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class BaseplateSetupService : ISetup
    {
        private readonly GameObject worldPrefab;
        private readonly LayerMask worldLayerMask;

        public BaseplateSetupService(WorldPrefabProvider worldPrefabProvider, Config config)
        {
            worldPrefab = worldPrefabProvider.zoneAreaPrefab;
            worldLayerMask = config.PlaceableLayerMask;
        }

        public void Setup()
        {
            var worldInstance = Object.Instantiate(worldPrefab);
            worldInstance.name = "World";
            worldInstance.layer = worldLayerMask;
            worldInstance.transform.position = Vector3.zero;
        }
    }
}
