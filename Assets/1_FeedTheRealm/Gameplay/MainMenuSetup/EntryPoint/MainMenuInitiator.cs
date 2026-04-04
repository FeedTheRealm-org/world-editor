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
        private InputReader inputReader;

        [SerializeField]
        private WorldSelector worldSelector;

        [Header("Component Providers")]
        [SerializeField]
        private MainMenuUIObjectProvider mainMenuUIObjectProvider;

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
            builder.Register<MainMenuUISetupService>(Lifetime.Scoped);

            builder.RegisterInstance(mainMenuUIObjectProvider);
            builder.RegisterInstance(config);
            builder.RegisterInstance(logger);
            builder.RegisterInstance(inputReader);
            builder.RegisterInstance(worldSelector);

            builder.RegisterEntryPoint<MainMenuEntrypoint>();
        }
    }
}
