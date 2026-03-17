using System;
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
        private PlaceablesLibrary placeableObjectLibrary;

        [SerializeField]
        private CreatorObjectLibrarySO creatorObjectLibrary;

        [Header("Event Channels")]
        [SerializeField]
        private EventChannelRegistry eventChannelRegistry;

        private readonly SetupServices setupServices = new();

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateSerializedFields();
            RegisterSerializedFields(builder);
            eventChannelRegistry.RegisterAll(builder);
            setupServices.RegisterAll(builder);
            builder.Register<WorldLoaderManager>(Lifetime.Scoped);
            builder.RegisterEntryPoint<WorldEditorEntrypoint>();
        }

        private void ValidateSerializedFields()
        {
            ValidateField(dataPersistenceManager);
            ValidateField(inputReader);
            ValidateField(worldPrefabProvider);
            ValidateField(WorldUIObjectProvider);
            ValidateField(placeableObjectLibrary);
            ValidateField(creatorObjectLibrary);
            ValidateField(playerConfig);
            ValidateField(eventChannelRegistry);
        }

        private void RegisterSerializedFields(IContainerBuilder builder)
        {
            builder.RegisterInstance(inputReader);
            builder.RegisterInstance(dataPersistenceManager);
            builder.RegisterInstance(worldPrefabProvider);
            builder.RegisterInstance(WorldUIObjectProvider);
            builder.RegisterInstance(placeableObjectLibrary);
            builder.RegisterInstance(creatorObjectLibrary);
            builder.RegisterInstance(playerConfig);
        }

        private void ValidateField(object field)
        {
            if (field == null)
                throw new NullReferenceException(
                    $"{nameof(field)} is not assigned in the Inspector."
                );
        }
    }
}
