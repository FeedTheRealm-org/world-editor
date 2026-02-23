using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class BaseplateSetupService : ISetup
    {
        private readonly GameObject worldPrefab;

        public BaseplateSetupService(WorldPrefabProvider worldPrefabProvider)
        {
            worldPrefab = worldPrefabProvider.worldPrefab;
        }

        public void Setup()
        {
            var worldInstance = Object.Instantiate(worldPrefab);
            worldInstance.gameObject.name = "World";
            worldInstance.gameObject.transform.position = Vector3.zero;
        }
    }
}
