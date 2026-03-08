using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class WorldUISetupService : SetupService
    {
        private readonly GameObject menuBarGameObject;
        private readonly GameObject editorBarGameObject;
        private readonly GameObject placeableDisplayObject;
        private readonly GameObject editorSettingsMenuObject;
        private readonly IObjectResolver objectResolver;
        private readonly GameObject loginMenuObject;
        private readonly GameObject signUpMenuObject;
        private readonly GameObject verifyCodeMenuObject;

        public WorldUISetupService(
            WorldUIObjectProvider WorldUIObjectProvider,
            IObjectResolver objectResolver,
            WorldSetupEvent setupEvent
        )
            : base(setupEvent)
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
            loginMenuObject = WorldUIObjectProvider.loginMenuObject;
            signUpMenuObject = WorldUIObjectProvider.signUpMenuObject;
            verifyCodeMenuObject = WorldUIObjectProvider.verifyCodeMenuObject;
            this.objectResolver = objectResolver;
        }

        public override void Setup()
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

            if (loginMenuObject == null)
                throw new System.Exception(
                    "LoginMenu GameObject not set in WorldUIObjectProvider!"
                );

            if (signUpMenuObject == null)
                throw new System.Exception(
                    "SignUpMenu GameObject not set in WorldUIObjectProvider!"
                );

            if (verifyCodeMenuObject == null)
                throw new System.Exception(
                    "VerifyCodeMenu GameObject not set in WorldUIObjectProvider!"
                );
        }
    }
}
