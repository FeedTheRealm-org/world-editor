using System;
using API;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldEditor;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FeedTheRealm.Gameplay.Player;
using FeedTheRealm.Gameplay.WorldLoader;
using FeedTheRealm.Gameplay.WorldSetup;
using FTR.Core.Common.Config;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Entrypoint.Scopes
{
    public class WorldEditorInitiator : LifetimeScope
    {
        [Header("Managers and Config")]
        [SerializeField]
        private WorldSelector worldSelector;

        [SerializeField]
        private InputReader inputReader;

        [SerializeField]
        private Config config;

        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private PlayerConfig playerConfig;

        [Header("Component Providers")]
        [SerializeField]
        private WorldPrefabProvider worldPrefabProvider;

        [SerializeField]
        private WorldUIObjectProvider WorldUIObjectProvider;

        [Header("API Services")]
        [SerializeField]
        private GltfService gltfService;

        [SerializeField]
        private AssetsService assetsService;

        [SerializeField]
        private ModelService modelService;

        [SerializeField]
        private MaterialService materialService;

        [SerializeField]
        private PlayerService playerService;

        [SerializeField]
        private Session.Session session;

        [Header("Event Channels")]
        [SerializeField]
        private EventChannelRegistry eventChannelRegistry;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterSerializedFields(builder);
            eventChannelRegistry.RegisterAll(builder);

            // Creatables Manager
            builder.Register<CreatablesManager>(Lifetime.Singleton);

            // Data persistence Manager
            builder.Register<DataPersistenceManager>(Lifetime.Singleton);

            builder.Register<ZoneManager>(Lifetime.Singleton);

            // Repositories
            builder.Register<ModelsRepository>(Lifetime.Singleton);
            builder.Register<WorldsRepository>(Lifetime.Singleton);
            builder.Register<CreatablesRepository>(Lifetime.Singleton);
            builder.Register<ZonesRepository>(Lifetime.Singleton);
            builder
                .Register<PlayerInfoRepository>(Lifetime.Singleton)
                .As<CharacterInfoRepository>();
            builder.Register<ZoneMaterialsRepository>(Lifetime.Singleton);

            // Libraries
            builder.Register<StructureLibrary>(Lifetime.Singleton);
            builder.Register<SpawnerLibrary>(Lifetime.Singleton);
            builder.Register<MiscLibrary>(Lifetime.Singleton);
            builder.Register<PlaceablesLibrary>(Lifetime.Singleton);

            // Loaders
            builder.Register<PlayerSpawnpointLoader>(Lifetime.Scoped);
            builder.Register<StructureLoader>(Lifetime.Scoped);
            builder.Register<AggresiveNpcSpawnerLoader>(Lifetime.Scoped);
            builder.Register<FriendlyNpcSpawnerLoader>(Lifetime.Scoped);
            builder.Register<PortalLoader>(Lifetime.Scoped);
            builder.Register<ChestLoader>(Lifetime.Scoped);

            builder.Register<CreatablesLoader>(Lifetime.Scoped);
            builder.Register<ZoneLoader>(Lifetime.Scoped);

            // World Setup Services
            builder.Register<CameraSetupService>(Lifetime.Scoped);
            builder.Register<PlayerSetupService>(Lifetime.Scoped);
            builder.Register<WorldEditorSetupService>(Lifetime.Scoped);
            builder.Register<WorldUISetupService>(Lifetime.Scoped);
            builder.Register<PlaceableEditorSetupService>(Lifetime.Scoped);

            builder.Register<WorldSetupManager>(Lifetime.Scoped);

            builder.RegisterEntryPoint<WorldEditorEntrypoint>();
        }

        private void RegisterSerializedFields(IContainerBuilder builder)
        {
            builder.RegisterInstance(inputReader);
            builder.RegisterInstance(worldSelector);
            builder.RegisterInstance(worldPrefabProvider);
            builder.RegisterInstance(WorldUIObjectProvider);
            builder.RegisterInstance(playerConfig);
            builder.RegisterInstance(gltfService);
            builder.RegisterInstance(assetsService);
            builder.RegisterInstance(playerService);
            builder.RegisterInstance(session);
            builder.RegisterInstance(modelService);
            builder.RegisterInstance(materialService);
            builder.RegisterInstance(config);
            builder.RegisterInstance(logger);
        }
    }
}
