using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.WorldObjects.Provider;
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
        private readonly IObjectResolver objectResolver;
        private readonly GameObject logingMenuObject;
        private readonly GameObject signUpMenuObject;
        private readonly GameObject verifyCodeMenuObject;

        public WorldUISetupService(
            WorldUIObjectProvider WorldUIObjectProvider,
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
            logingMenuObject = WorldUIObjectProvider.logingMenuObject;
            signUpMenuObject = WorldUIObjectProvider.signUpMenuObject;
            verifyCodeMenuObject = WorldUIObjectProvider.verifyCodeMenuObject;
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

            if (logingMenuObject == null)
                throw new System.Exception(
                    "LogingMenu GameObject not set in WorldUIObjectProvider!"
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
