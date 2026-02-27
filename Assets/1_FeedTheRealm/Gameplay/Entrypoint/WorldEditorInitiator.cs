using FeedTheRealm.Core;
using FeedTheRealm.Core.EventChannels.Ticks;
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
        private TickEvent tickEvent;

        [SerializeField]
        private FixedTickEvent fixedTickEvent;

        [SerializeField]
        private LateTickEvent lateTickEvent;

        [SerializeField]
        private PlayerConfig playerConfig;

        [SerializeField]
        private UIObjectProvider uIObjectProvider;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(dataPersistenceManager);
            builder.RegisterInstance(InputReader);
            builder.RegisterInstance(worldPrefabProvider);
            builder.RegisterInstance(uIObjectProvider);

            builder.RegisterInstance(tickEvent);
            builder.RegisterInstance(fixedTickEvent);
            builder.RegisterInstance(lateTickEvent);
            builder.RegisterInstance(playerConfig);

            builder.Register<BaseplateSetupService>(Lifetime.Scoped);
            builder.Register<CameraSetupService>(Lifetime.Scoped);
            builder.Register<LightingSetupService>(Lifetime.Scoped);
            builder.Register<PlayerSetupService>(Lifetime.Scoped);
            builder.Register<WorldEditorSetupService>(Lifetime.Scoped);
            builder.Register<UISetupService>(Lifetime.Scoped);

            builder.Register<WorldSetupService>(Lifetime.Scoped);
            builder.Register<WorldLoader>(Lifetime.Scoped);

            builder.RegisterEntryPoint<WorldEditorEntrypoint>();
        }
    }
}
