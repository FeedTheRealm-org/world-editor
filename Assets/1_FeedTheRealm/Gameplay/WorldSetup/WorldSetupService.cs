using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldSetupService
    {
        private readonly WorldSetupEvent setupEvent;
        private readonly IEnumerable<ISetup> setupServices;

        public WorldSetupService(WorldSetupEvent setupEvent, IEnumerable<ISetup> setupServices)
        {
            this.setupEvent = setupEvent;
            this.setupServices = setupServices;
        }

        public void ExecuteSetup()
        {
            Debug.Log("Executing World Setup...");
            setupEvent.Raise();
        }
    }
}
