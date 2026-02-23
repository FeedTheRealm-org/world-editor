using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class LightingSetupService : ISetup
    {
        public void Setup()
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
}
