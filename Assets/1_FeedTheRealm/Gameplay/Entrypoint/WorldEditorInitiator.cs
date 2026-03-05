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
        private UIObjectProvider uIObjectProvider;

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
            ValidateField(uIObjectProvider, nameof(uIObjectProvider));
            ValidateField(placeableObjectLibrary, nameof(placeableObjectLibrary));
            ValidateField(creatorObjectLibrary, nameof(creatorObjectLibrary));
            ValidateField(playerConfig, nameof(playerConfig));
            ValidateField(eventChannelRegistry, nameof(eventChannelRegistry));

            builder.RegisterInstance(inputReader);
            builder.RegisterInstance(dataPersistenceManager);
            builder.RegisterInstance(worldPrefabProvider);
            builder.RegisterInstance(uIObjectProvider);
            builder.RegisterInstance(placeableObjectLibrary);
            builder.RegisterInstance(creatorObjectLibrary);
            builder.RegisterInstance(playerConfig);
        }

        private void ValidateField(object field, string fieldName)
        {
            if (field == null)
                throw new System.NullReferenceException(
                    $"[WorldEditorInitiator] {fieldName} is not assigned in the Inspector."
                );
        }
    }
}
