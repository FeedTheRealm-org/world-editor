using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class UISetupService : SetupService
    {
        private readonly GameObject menuBarGameObject;
        private readonly GameObject editorBarGameObject;
        private readonly GameObject placeableDisplayObject;
        private readonly GameObject editorSettingsMenuObject;
        private readonly IObjectResolver objectResolver;

        public UISetupService(
            UIObjectProvider UIObjectProvider,
            IObjectResolver objectResolver,
            WorldSetupEvent setupEvent
        )
            : base(setupEvent)
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

        public override void Setup()
        {
            if (menuBarGameObject == null)
            {
                Debug.LogError("MenuBar GameObject not set in UIObjectProvider!");
                return;
            }
            objectResolver.Instantiate(menuBarGameObject).name = "MenuBar";

            if (editorBarGameObject == null)
            {
                Debug.LogError("EditorBar GameObject not set in UIObjectProvider!");
                return;
            }
            objectResolver.Instantiate(editorBarGameObject).name = "EditorBar";

            if (placeableDisplayObject == null)
            {
                Debug.LogError("PlaceableDisplay GameObject not set in UIObjectProvider!");
                return;
            }
            objectResolver.Instantiate(placeableDisplayObject).name = "PlaceableDisplay";

            if (editorSettingsMenuObject == null)
            {
                Debug.LogError("EditorSettingsMenu GameObject not set in UIObjectProvider!");
                return;
            }
        }
    }
}
