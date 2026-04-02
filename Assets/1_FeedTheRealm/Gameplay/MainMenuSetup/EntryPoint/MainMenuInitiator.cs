using API;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.Library;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FeedTheRealm.Gameplay.MainMenuSetup.Services;
using FeedTheRealm.Gameplay.WorldLoader;
using FTR.Core.Common.Config;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.MainMenuSetup.Entrypoint
{
    public class MainMenuInitiator : LifetimeScope
    {
        [Header("Services, Managers and Config")]
        [SerializeField]
        private Config config;

        [SerializeField]
        private Logging.Logger logger;

        [SerializeField]
        private WorldSelector worldSelector;

        [SerializeField]
        private InputReader inputReader;

        [Header("Component Providers")]
        [SerializeField]
        private MainMenuUIObjectProvider mainMenuUIObjectProvider;

        [Header("Component Providers")]
        [SerializeField]
        private WorldPrefabProvider worldPrefabProvider;

        [Header("API Services")]
        [SerializeField]
        private GltfService gltfService;

        [Header("Event Channels")]
        [SerializeField]
        private EventChannelRegistry eventChannelRegistry;

        protected override void Configure(IContainerBuilder builder)
        {
            //TODO: all of these registrations are the same as in WorldEditorInitiator,
            // the main reason that we had to do this is due to the menu bar having dependencies, and those dependencies depend
            // on other components and so on,
            // we should find a way to avoid this repetition, maybe by creating a common initiator for shared dependencies?
            eventChannelRegistry.RegisterAll(builder);

            // Creatables Manager
            builder.Register<CreatablesManager>(Lifetime.Singleton);

            // Data persistence Manager
            builder.Register<DataPersistenceManager>(Lifetime.Singleton);

            // Repositories
            builder.Register<ModelsRepository>(Lifetime.Singleton);
            builder.Register<WorldsRepository>(Lifetime.Singleton);
            builder.Register<CreatablesRepository>(Lifetime.Singleton);
            builder.Register<ZonesRepository>(Lifetime.Singleton);

            // Libraries
            builder.Register<StructureLibrary>(Lifetime.Singleton);
            builder.Register<SpawnerLibrary>(Lifetime.Singleton);
            builder.Register<PlaceablesLibrary>(Lifetime.Singleton);

            // Loaders
            builder.Register<PlayerSpawnpointLoader>(Lifetime.Scoped);
            builder.Register<StructureLoader>(Lifetime.Scoped);
            builder.Register<AggresiveNpcSpawnerLoader>(Lifetime.Scoped);
            builder.Register<FriendlyNpcSpawnerLoader>(Lifetime.Scoped);

            builder.Register<CreatablesLoader>(Lifetime.Scoped);
            builder.Register<ZoneLoader>(Lifetime.Scoped);

            builder.Register<MainMenuUISetupService>(Lifetime.Scoped);

            builder.RegisterInstance(mainMenuUIObjectProvider);
            builder.RegisterInstance(config);
            builder.RegisterInstance(logger);
            builder.RegisterInstance(worldSelector);
            builder.RegisterInstance(inputReader);
            builder.RegisterInstance(gltfService);
            builder.RegisterInstance(worldPrefabProvider);

            builder.RegisterEntryPoint<MainMenuEntrypoint>();
        }
    }
}
