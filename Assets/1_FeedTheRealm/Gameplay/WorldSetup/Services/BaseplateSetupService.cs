using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class BaseplateSetupService : SetupService
    {
        private readonly GameObject worldPrefab;
        private readonly LayerMask worldLayerMask;

        public BaseplateSetupService(
            WorldPrefabProvider worldPrefabProvider,
            WorldSetupEvent setupEvent
        )
            : base(setupEvent)
        {
            worldPrefab = worldPrefabProvider.worldPrefab;
            worldLayerMask = worldPrefabProvider.worldLayerMask;
        }

        public override void Setup()
        {
            var worldInstance = Object.Instantiate(worldPrefab);
            worldInstance.name = "World";
            worldInstance.layer = Mathf.RoundToInt(Mathf.Log(worldLayerMask.value, 2));
            worldInstance.transform.position = Vector3.zero;
        }
    }
}
