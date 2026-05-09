using System.Collections.Generic;
using FeedTheRealm.Core.WorldSetup;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldSetupManager
    {
        private readonly List<ISetup> setupServices;
        private readonly Logging.Logger logger;

        public WorldSetupManager(
            Logging.Logger logger,
            CameraSetupService cameraSetupService,
            PlayerSetupService playerSetupService,
            WorldEditorSetupService worldEditorSetupService,
            WorldUISetupService worldUISetupService,
            PlaceableEditorSetupService placeableEditorSetupService
        )
        {
            this.logger = logger;
            setupServices = new List<ISetup>
            {
                cameraSetupService,
                playerSetupService,
                worldEditorSetupService,
                worldUISetupService,
                placeableEditorSetupService,
            };
        }

        public void ExecuteSetup()
        {
            logger.Log("Starting world setup...");
            foreach (var service in setupServices)
                service.Setup();
        }
    }
}
