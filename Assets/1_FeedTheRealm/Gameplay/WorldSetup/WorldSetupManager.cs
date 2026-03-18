using System.Collections.Generic;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldSetup;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldSetupManager
    {
        private readonly WorldSetupEvent setupEvent;

        public WorldSetupManager(WorldSetupEvent setupEvent, IEnumerable<ISetup> setupServices)
        {
            this.setupEvent = setupEvent;
        }

        public void ExecuteSetup()
        {
            Debug.Log("Executing World Setup...");
            setupEvent.Raise();
        }
    }
}
