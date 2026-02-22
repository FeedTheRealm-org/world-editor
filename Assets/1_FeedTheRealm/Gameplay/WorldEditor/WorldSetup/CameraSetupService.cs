using Unity.Cinemachine;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class CameraSetupService
    {
        public Camera Setup(Transform followTarget)
        {
            // Create main camera
            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";

            // Add Cinemachine Brain
            cameraGO.AddComponent<CinemachineBrain>();

            // Create virtual camera
            var vcamGO = new GameObject("World Virtual Camera");
            var virtualCamera = vcamGO.AddComponent<CinemachineCamera>();

            virtualCamera.Follow = followTarget;
            virtualCamera.LookAt = followTarget;

            // Position the virtual camera
            virtualCamera.transform.position = new Vector3(0, 10, -10);

            return camera;
        }
    }
}
