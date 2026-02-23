using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldSetupService
    {
        private readonly WorldCreatorService worldFactory;
        private readonly CameraSetupService cameraSetup;
        private readonly LightingSetupService lightingSetup;

        public WorldSetupService(
            WorldCreatorService worldFactory,
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
            worldFactory.Create();
            cameraSetup.Setup();
            lightingSetup.Setup();
        }
    }
}
