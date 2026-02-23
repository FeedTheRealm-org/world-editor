using FeedTheRealm.Core.Interfaces;
using Unity.Cinemachine;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class CameraSetupService : ISetup
    {
        public void Setup()
        {
            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            cameraGO.AddComponent<CinemachineBrain>();
            var vcamGO = new GameObject("World Virtual Camera");
            var virtualCamera = vcamGO.AddComponent<CinemachineCamera>();
            virtualCamera.transform.position = new Vector3(0, 10, -10);
        }
    }
}
