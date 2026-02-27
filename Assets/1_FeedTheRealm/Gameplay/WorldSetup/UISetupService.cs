using FeedTheRealm.Core.Interfaces;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class UISetupService : ISetup
    {
        private readonly GameObject menuBarGameObject;
        private readonly GameObject editorBarGameObject;
        private readonly GameObject placeableDisplayObject;
        private readonly GameObject editorSettingsMenuObject;
        private readonly IObjectResolver objectResolver;

        public UISetupService(UIObjectProvider UIObjectProvider, IObjectResolver objectResolver)
        {
            if (UIObjectProvider == null)
            {
                Debug.LogError("UIObjectProvider not set!");
                return;
            }
            menuBarGameObject = UIObjectProvider.menuBarGameObject;
            editorBarGameObject = UIObjectProvider.editorBarGameObject;
            placeableDisplayObject = UIObjectProvider.placeableDisplayObject;
            editorSettingsMenuObject = UIObjectProvider.editorSettingsMenuObject;
            this.objectResolver = objectResolver;
        }

        public void Setup()
        {
            if (menuBarGameObject == null)
            {
                Debug.LogError("MenuBar GameObject not set in UIObjectProvider!");
                return;
            }
            objectResolver.InjectGameObject(menuBarGameObject);
            GameObject menuBarInstance = Object.Instantiate(menuBarGameObject);
            menuBarInstance.name = "MenuBar";

            if (editorBarGameObject == null)
            {
                Debug.LogError("EditorBar GameObject not set in UIObjectProvider!");
                return;
            }
            objectResolver.InjectGameObject(editorBarGameObject);
            GameObject editorBarInstance = Object.Instantiate(editorBarGameObject);
            editorBarInstance.name = "EditorBar";
            if (placeableDisplayObject == null)
            {
                Debug.LogError("PlaceableDisplay GameObject not set in UIObjectProvider!");
                return;
            }
            objectResolver.InjectGameObject(placeableDisplayObject);
            Object.Instantiate(placeableDisplayObject).name = "PlaceableDisplay";

            if (editorSettingsMenuObject == null)
            {
                Debug.LogError("EditorSettingsMenu GameObject not set in UIObjectProvider!");
                return;
            }
            objectResolver.InjectGameObject(editorSettingsMenuObject);
            Object.Instantiate(editorSettingsMenuObject).name = "EditorSettingsMenu";
        }
    }
}
