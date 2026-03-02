using FeedTheRealm.Core;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.WorldSetup;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldEditorInitiator : LifetimeScope
    {
        [SerializeField]
        private DataPersistenceManagerSO dataPersistenceManager;

        [SerializeField]
        private InputReader InputReader;

        [SerializeField]
        private WorldPrefabProvider worldPrefabProvider;

        [SerializeField]
        private PlayerConfig playerConfig;

        [SerializeField]
        private UIObjectProvider uIObjectProvider;

        [SerializeField]
        private PlaceableObjectsLibrarySO placeableObjectLibrary;

        [SerializeField]
        private CreatorObjectLibrarySO creatorObjectLibrary;

        [SerializeField]
        private EventChannelRegistry eventChannelRegistry;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateSerializedFields();

            builder.RegisterInstance(dataPersistenceManager);
            builder.RegisterInstance(InputReader);
            builder.RegisterInstance(worldPrefabProvider);
            builder.RegisterInstance(uIObjectProvider);
            builder.RegisterInstance(placeableObjectLibrary);
            builder.RegisterInstance(creatorObjectLibrary);
            builder.RegisterInstance(playerConfig);

            eventChannelRegistry.RegisterAll(builder);

            builder.Register<BaseplateSetupService>(Lifetime.Scoped);
            builder.Register<CameraSetupService>(Lifetime.Scoped);
            builder.Register<LightingSetupService>(Lifetime.Scoped);
            builder.Register<PlayerSetupService>(Lifetime.Scoped);
            builder.Register<LibrarySetupService>(Lifetime.Scoped);
            builder.Register<WorldEditorSetupService>(Lifetime.Scoped);
            builder.Register<UISetupService>(Lifetime.Scoped);

            builder.Register<WorldSetupService>(Lifetime.Scoped);
            builder.Register<WorldLoader>(Lifetime.Scoped);

            builder.RegisterEntryPoint<WorldEditorEntrypoint>();
        }

        private void ValidateSerializedFields()
        {
            if (dataPersistenceManager == null)
                Debug.LogError(
                    $"[WorldEditorInitiator] {nameof(dataPersistenceManager)} is not assigned in the Inspector."
                );
            if (InputReader == null)
                Debug.LogError(
                    $"[WorldEditorInitiator] {nameof(InputReader)} is not assigned in the Inspector."
                );
            if (worldPrefabProvider == null)
                Debug.LogError(
                    $"[WorldEditorInitiator] {nameof(worldPrefabProvider)} is not assigned in the Inspector."
                );
            if (uIObjectProvider == null)
                Debug.LogError(
                    $"[WorldEditorInitiator] {nameof(uIObjectProvider)} is not assigned in the Inspector."
                );
            if (placeableObjectLibrary == null)
                Debug.LogError(
                    $"[WorldEditorInitiator] {nameof(placeableObjectLibrary)} is not assigned in the Inspector."
                );
            if (creatorObjectLibrary == null)
                Debug.LogError(
                    $"[WorldEditorInitiator] {nameof(creatorObjectLibrary)} is not assigned in the Inspector."
                );
            if (playerConfig == null)
                Debug.LogError(
                    $"[WorldEditorInitiator] {nameof(playerConfig)} is not assigned in the Inspector."
                );
            if (eventChannelRegistry == null)
                Debug.LogError(
                    $"[WorldEditorInitiator] {nameof(eventChannelRegistry)} is not assigned in the Inspector."
                );
        }
    }
}
