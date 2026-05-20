using FeedTheRealm.Core.EventChannels.UIEvents;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Core.WorldSetup;
using FTRShared.UI.AuthMenu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldUISetupService : ISetup
    {
        private readonly GameObject menuBarGameObject;
        private readonly GameObject editorBarGameObject;
        private readonly GameObject placeableDisplayObject;
        private readonly GameObject editorSettingsMenuObject;
        private readonly GameObject subscriptionMenuObject;
        private readonly AuthFlowManager authFlowManager;
        private readonly UpdateLoginEvent updateLoginEvent;
        private readonly IObjectResolver objectResolver;

        public WorldUISetupService(
            WorldUIObjectProvider WorldUIObjectProvider,
            UpdateLoginEvent updateLoginEvent,
            AuthFlowManager authFlowManager,
            IObjectResolver objectResolver
        )
        {
            if (WorldUIObjectProvider == null)
            {
                Debug.LogError("WorldUIObjectProvider not set!");
                return;
            }
            menuBarGameObject = WorldUIObjectProvider.menuBarGameObject;
            editorBarGameObject = WorldUIObjectProvider.editorBarGameObject;
            placeableDisplayObject = WorldUIObjectProvider.placeableDisplayObject;
            editorSettingsMenuObject = WorldUIObjectProvider.editorSettingsMenuObject;
            subscriptionMenuObject = WorldUIObjectProvider.subscriptionMenuObject;
            this.updateLoginEvent = updateLoginEvent;
            this.authFlowManager = authFlowManager;
            this.objectResolver = objectResolver;
        }

        public void Setup()
        {
            if (menuBarGameObject == null)
                throw new System.Exception("MenuBar GameObject not set in WorldUIObjectProvider!");
            objectResolver.Instantiate(menuBarGameObject).name = "MenuBar";

            if (editorBarGameObject == null)
                throw new System.Exception(
                    "EditorBar GameObject not set in WorldUIObjectProvider!"
                );
            objectResolver.Instantiate(editorBarGameObject).name = "EditorBar";

            if (placeableDisplayObject == null)
                throw new System.Exception(
                    "PlaceableDisplay GameObject not set in WorldUIObjectProvider!"
                );
            objectResolver.Instantiate(placeableDisplayObject).name = "PlaceableDisplay";

            if (editorSettingsMenuObject == null)
                throw new System.Exception(
                    "EditorSettingsMenu GameObject not set in WorldUIObjectProvider!"
                );

            if (subscriptionMenuObject == null)
                throw new System.Exception(
                    "SubscriptionMenu GameObject not set in WorldUIObjectProvider!"
                );
            //objectResolver.Instantiate(subscriptionMenuObject).name = "SubscriptionMenu";

            authFlowManager.OnAuthComplete += (message) =>
            {
                ToastNotification.Show(message, "success", Color.green);
                updateLoginEvent.Raise();
            };
            authFlowManager.OnPasswordResetComplete += (message) =>
            {
                ToastNotification.Show(message, "success", Color.green);
            };
        }
    }
}
