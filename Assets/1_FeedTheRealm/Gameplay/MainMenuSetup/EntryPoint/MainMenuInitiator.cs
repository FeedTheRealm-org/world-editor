using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.MainMenuSetup.Services;
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

        [Header("Event Channels")]
        [SerializeField]
        private EventChannelRegistry eventChannelRegistry;

        protected override void Configure(IContainerBuilder builder)
        {
            eventChannelRegistry.RegisterAll(builder);
            builder.Register<MainMenuUISetupService>(Lifetime.Scoped);
            builder.Register<DataPersistenceManager>(Lifetime.Singleton);

            builder.RegisterInstance(mainMenuUIObjectProvider);
            builder.RegisterInstance(config);
            builder.RegisterInstance(logger);
            builder.RegisterInstance(worldSelector);
            builder.RegisterInstance(inputReader);

            builder.RegisterEntryPoint<MainMenuEntrypoint>();
        }
    }
}
