using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class LightingSetupService
    {
        public Light Setup()
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();

            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            return light;
        }
    }
}
