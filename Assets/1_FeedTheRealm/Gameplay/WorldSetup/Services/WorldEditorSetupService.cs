using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Core.WorldSetup;
using FeedTheRealm.Gameplay.WorldEditor.WorldEditorStateMachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldEditorSetupService : ISetup
    {
        private readonly GameObject worldEditorPrefab;
        private readonly IObjectResolver objectResolver;
        private readonly CameraSetupService cameraSetup;

        public WorldEditorSetupService(
            WorldPrefabProvider worldPrefabProvider,
            CameraSetupService cameraSetup,
            IObjectResolver objectResolver
        )
        {
            if (worldPrefabProvider == null)
            {
                Debug.LogError("World prefab not set!");
                return;
            }
            worldEditorPrefab = worldPrefabProvider.worldEditorPrefab;
            this.objectResolver = objectResolver;
            this.cameraSetup = cameraSetup;
        }

        public void Setup()
        {
            GameObject worldEditorInstance = Object.Instantiate(
                worldEditorPrefab,
                Vector3.zero,
                Quaternion.identity
            );
            worldEditorInstance.name = "World Editor";
            worldEditorInstance.SetActive(false);
            objectResolver.InjectGameObject(worldEditorInstance);
            worldEditorInstance.GetComponent<WorldEditorStateMachine>().playerCamera =
                cameraSetup.MainCamera;
            worldEditorInstance.SetActive(true);
        }
    }
}
