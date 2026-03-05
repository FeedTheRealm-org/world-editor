using FeedTheRealm.Core.EventChannels.WorldEvents;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class LightingSetupService : SetupService
    {
        public LightingSetupService(WorldSetupEvent setupEvent)
            : base(setupEvent) { }

        public override void Setup()
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
}
