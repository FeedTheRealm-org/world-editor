using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.Provider;
using FeedTheRealm.Core.WorldSetup;
using FeedTheRealm.Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class PlaceableEditorSetupService : ISetup
    {
        private readonly GameObject placeableEditor;
        private readonly IObjectResolver objectResolver;

        public PlaceableEditorSetupService(
            WorldUIObjectProvider worldUIPrefabProvider,
            IObjectResolver objectResolver
        )
        {
            if (worldUIPrefabProvider == null)
            {
                Debug.LogError("World prefab not set!");
                return;
            }
            placeableEditor = worldUIPrefabProvider.placeableEditor;
            this.objectResolver = objectResolver;
        }

        public void Setup()
        {
            var instance = objectResolver.Instantiate(placeableEditor);
            instance.name = "PlaceableEditor";
        }
    }
}
