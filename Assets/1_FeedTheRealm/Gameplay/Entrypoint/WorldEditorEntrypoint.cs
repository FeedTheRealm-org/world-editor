using FeedTheRealm.Gameplay.Inputs;
using FeedTheRealm.Gameplay.WorldSetup;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldEditorEntrypoint : LifetimeScope
    {
        [SerializeField]
        private DataPersistenceManagerSO dataPersistenceManager;

        [SerializeField]
        private InputReader InputReader;

        [SerializeField]
        private WorldControllerV2 WorldControllerV2Prefab;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(dataPersistenceManager);
            builder.RegisterInstance(InputReader);

            builder.RegisterInstance(WorldControllerV2Prefab);
            builder.Register<BaseplateSetupService>(Lifetime.Scoped);
            builder.Register<CameraSetupService>(Lifetime.Scoped);
            builder.Register<LightingSetupService>(Lifetime.Scoped);

            builder.Register<WorldSetupService>(Lifetime.Scoped);
            builder.Register<WorldLoader>(Lifetime.Scoped);

            builder.RegisterEntryPoint<WorldEditorInitiator>();
        }
    }
}
