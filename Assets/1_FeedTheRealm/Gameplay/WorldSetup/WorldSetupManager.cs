using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldSetup;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldSetupManager
    {
        readonly List<ISetup> setupServices;

        public WorldSetupManager(
            BaseplateSetupService baseplateSetupService,
            CameraSetupService cameraSetupService,
            LightingSetupService lightingSetupService,
            PlayerSetupService playerSetupService,
            WorldEditorSetupService worldEditorSetupService,
            WorldUISetupService worldUISetupService
        )
        {
            setupServices = new List<ISetup>
            {
                baseplateSetupService,
                cameraSetupService,
                lightingSetupService,
                playerSetupService,
                worldEditorSetupService,
                worldUISetupService,
            };
        }

        public void ExecuteSetup()
        {
            Debug.Log("Executing World Setup...");
            foreach (var service in setupServices)
                service.Setup();
        }
    }
}
