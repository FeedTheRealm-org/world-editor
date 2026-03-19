using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldSetup;
using Unity.Cinemachine;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class CameraSetupService : ISetup
    {
        public Camera MainCamera { get; private set; }

        public CameraSetupService() { }

        public void Setup()
        {
            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            cameraGO.AddComponent<CinemachineBrain>();
            MainCamera = camera;
        }
    }
}
