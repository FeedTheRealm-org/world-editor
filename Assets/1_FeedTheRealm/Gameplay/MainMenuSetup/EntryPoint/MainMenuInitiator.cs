using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Gameplay.MainMenuSetup.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.MainMenuSetup.Entrypoint
{
    public class MainMenuInitiator : LifetimeScope
    {
        [Header("Services, Managers and Config")]
        [SerializeField]
        private DataPersistenceManagerSO dataPersistenceManager;

        [Header("Component Providers")]
        [SerializeField]
        private MainMenuUIObjectProvider mainMenuUIObjectProvider;

        [Header("Event Channels")]
        [SerializeField]
        private EventChannelRegistry eventChannelRegistry;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateSerializedFields();
            builder.RegisterInstance(mainMenuUIObjectProvider);

            eventChannelRegistry.RegisterAll(builder);

            builder.Register<MainMenuUISetupService>(Lifetime.Scoped);

            builder.RegisterEntryPoint<MainMenuEntrypoint>();
        }

        private void ValidateSerializedFields()
        {
            if (dataPersistenceManager == null)
                throw new System.NullReferenceException(
                    $"[MainMenuInitiator] {nameof(dataPersistenceManager)} is not assigned in the Inspector."
                );
            if (mainMenuUIObjectProvider == null)
                throw new System.NullReferenceException(
                    $"[MainMenuInitiator] {nameof(mainMenuUIObjectProvider)} is not assigned in the Inspector."
                );
            if (eventChannelRegistry == null)
                throw new System.NullReferenceException(
                    $"[MainMenuInitiator] {nameof(eventChannelRegistry)} is not assigned in the Inspector."
                );
        }
    }
}
