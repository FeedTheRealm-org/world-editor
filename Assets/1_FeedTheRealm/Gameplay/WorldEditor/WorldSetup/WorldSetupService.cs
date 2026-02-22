using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldSetupService
    {
        private readonly WorldFactory worldFactory;
        private readonly CameraSetupService cameraSetup;
        private readonly LightingSetupService lightingSetup;

        public WorldSetupService(
            WorldFactory worldFactory,
            CameraSetupService cameraSetup,
            LightingSetupService lightingSetup
        )
        {
            this.worldFactory = worldFactory;
            this.cameraSetup = cameraSetup;
            this.lightingSetup = lightingSetup;
        }

        public void Setup()
        {
            Debug.Log("Setting up world...");
            worldFactory.Create();
            cameraSetup.Setup();
            lightingSetup.Setup();
            Debug.Log("World setup complete.");
        }
    }
}
