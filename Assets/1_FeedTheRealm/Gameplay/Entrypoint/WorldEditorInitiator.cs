using FeedTheRealm.Core;
using FeedTheRealm.Core.DataPersistence;
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
        private UIObjectProvider uIObjectProvider;

        [Header("Libraries")]
        [SerializeField]
        private PlaceableObjectsLibrarySO placeableObjectLibrary;

        [SerializeField]
        private CreatorObjectLibrarySO creatorObjectLibrary;

        [Header("Event Channels")]
        [SerializeField]
        private EventChannelRegistry eventChannelRegistry;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(inputReader);
            builder.RegisterInstance(dataPersistenceManager);
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
    }
}
