using FeedTheRealm.Core;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.Library.CreatorObjectLibrary;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FeedTheRealm.Gameplay.Player;
using FeedTheRealm.Gameplay.WorldSetup;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldEditorInitiator : LifetimeScope
    {
        [Header("Services, Managers and Config")]
        [SerializeField]
        private DataPersistenceManagerSO dataPersistenceManager;

        [SerializeField]
        private InputReader inputReader;

        [SerializeField]
        private PlayerConfig playerConfig;

        [Header("Component Providers")]
        [SerializeField]
        private WorldPrefabProvider worldPrefabProvider;

        [SerializeField]
        private WorldUIObjectProvider WorldUIObjectProvider;

        [Header("Libraries")]
        [SerializeField]
        private PlaceableObjectsLibrarySO placeableObjectLibrary;

        [SerializeField]
        private CreatorObjectLibrarySO creatorObjectLibrary;

        [Header("Event Channels")]
        [SerializeField]
        private EventChannelRegistry eventChannelRegistry;

        // Internal Classes
        private readonly SetupServices setupServices = new();

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterSerializedFields(builder);
            eventChannelRegistry.RegisterAll(builder);
            setupServices.RegisterAll(builder);
            builder.Register<WorldLoader>(Lifetime.Scoped);
            builder.RegisterEntryPoint<WorldEditorEntrypoint>();
        }

        private void RegisterSerializedFields(IContainerBuilder builder)
        {
            ValidateField(dataPersistenceManager, nameof(dataPersistenceManager));
            ValidateField(inputReader, nameof(inputReader));
            ValidateField(worldPrefabProvider, nameof(worldPrefabProvider));
            ValidateField(WorldUIObjectProvider, nameof(WorldUIObjectProvider));
            ValidateField(placeableObjectLibrary, nameof(placeableObjectLibrary));
            ValidateField(creatorObjectLibrary, nameof(creatorObjectLibrary));
            ValidateField(playerConfig, nameof(playerConfig));
            ValidateField(eventChannelRegistry, nameof(eventChannelRegistry));

            builder.RegisterInstance(inputReader);
            builder.RegisterInstance(dataPersistenceManager);
            builder.RegisterInstance(worldPrefabProvider);
            builder.RegisterInstance(WorldUIObjectProvider);
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
            builder.Register<WorldUISetupService>(Lifetime.Scoped);

            builder.Register<WorldSetupService>(Lifetime.Scoped);
            builder.Register<WorldLoader>(Lifetime.Scoped);

            builder.RegisterEntryPoint<WorldEditorEntrypoint>();
        }

        private void ValidateField(object field, string fieldName)
        {
            if (field == null)
                throw new System.NullReferenceException(
                    $"{nameof(dataPersistenceManager)} is not assigned in the Inspector."
                );
            if (inputReader == null)
                throw new System.NullReferenceException(
                    $"{nameof(inputReader)} is not assigned in the Inspector."
                );
            if (worldPrefabProvider == null)
                throw new System.NullReferenceException(
                    $"{nameof(worldPrefabProvider)} is not assigned in the Inspector."
                );
            if (WorldUIObjectProvider == null)
                throw new System.NullReferenceException(
                    $"{nameof(WorldUIObjectProvider)} is not assigned in the Inspector."
                );
            if (placeableObjectLibrary == null)
                throw new System.NullReferenceException(
                    $"{nameof(placeableObjectLibrary)} is not assigned in the Inspector."
                );
            if (creatorObjectLibrary == null)
                throw new System.NullReferenceException(
                    $"{nameof(creatorObjectLibrary)} is not assigned in the Inspector."
                );
            if (playerConfig == null)
                throw new System.NullReferenceException(
                    $"{nameof(playerConfig)} is not assigned in the Inspector."
                );
            if (eventChannelRegistry == null)
                throw new System.NullReferenceException(
                    $"{nameof(eventChannelRegistry)} is not assigned in the Inspector."
                );
        }
    }
}
