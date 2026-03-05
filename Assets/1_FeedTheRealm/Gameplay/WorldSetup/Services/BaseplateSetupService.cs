using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class BaseplateSetupService : SetupService
    {
        private readonly GameObject worldPrefab;

        public BaseplateSetupService(
            WorldPrefabProvider worldPrefabProvider,
            WorldSetupEvent setupEvent
        )
            : base(setupEvent)
        {
            worldPrefab = worldPrefabProvider.worldPrefab;
        }

        public override void Setup()
        {
            var worldInstance = Object.Instantiate(worldPrefab);
            worldInstance.gameObject.name = "World";
            worldInstance.gameObject.transform.position = Vector3.zero;
        }
    }
}
