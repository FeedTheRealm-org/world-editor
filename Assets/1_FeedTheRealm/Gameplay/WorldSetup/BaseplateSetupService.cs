using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class BaseplateSetupService : ISetup
    {
        private readonly WorldControllerV2 worldController;

        public BaseplateSetupService(WorldControllerV2 worldController)
        {
            this.worldController = worldController;
        }

        public void Setup()
        {
            WorldControllerV2 worldInstance = Object.Instantiate(worldController);
            worldInstance.gameObject.name = "World";
            worldInstance.gameObject.transform.position = Vector3.zero;
        }
    }
}
