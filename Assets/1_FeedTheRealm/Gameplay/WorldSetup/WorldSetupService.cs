using System.Collections.Generic;
using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldSetupService
    {
        private readonly List<ISetup> setupServices;

        public WorldSetupService(
            BaseplateSetupService worldCreator,
            CameraSetupService cameraSetup,
            LightingSetupService lightingSetup,
            PlayerSetupService playerSetup,
            LibrarySetupService librarySetup,
            UISetupService uiSetup
        )
        {
            setupServices = new List<ISetup>
            {
                worldCreator,
                cameraSetup,
                lightingSetup,
                playerSetup,
                librarySetup,
                uiSetup,
            };
        }

        public void ExecuteSetup()
        {
            foreach (var service in setupServices)
            {
                service.Setup();
            }
        }
    }
}
