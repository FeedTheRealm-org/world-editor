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
            LightingSetupService lightingSetup
        )
        {
            setupServices = new List<ISetup> { worldCreator, cameraSetup, lightingSetup };
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
