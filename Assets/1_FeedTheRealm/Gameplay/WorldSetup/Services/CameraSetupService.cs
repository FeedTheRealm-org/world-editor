using FeedTheRealm.Core.EventChannels.WorldEvents;
using Unity.Cinemachine;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class CameraSetupService : SetupService
    {
        public Camera MainCamera { get; private set; }

        public CameraSetupService(WorldSetupEvent setupEvent)
            : base(setupEvent) { }

        public override void Setup()
        {
            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            cameraGO.AddComponent<CinemachineBrain>();
            MainCamera = camera;
        }
    }
}
