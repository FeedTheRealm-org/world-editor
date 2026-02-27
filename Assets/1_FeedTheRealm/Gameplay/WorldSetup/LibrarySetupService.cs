using FeedTheRealm.Core.Interfaces;
using UnityEngine;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public class LibrarySetupService : ISetup
    {
        private readonly PlaceableObjectsLibrarySO placeableObjectLibrary;
        private readonly CreatorObjectLibrarySO creatorObjectLibrary;

        public LibrarySetupService(
            PlaceableObjectsLibrarySO placeableObjectLibrary,
            CreatorObjectLibrarySO creatorObjectLibrary
        )
        {
            if (placeableObjectLibrary == null)
            {
                Debug.LogError("PlaceableObjectsLibrarySO not set!");
                return;
            }
            if (creatorObjectLibrary == null)
            {
                Debug.LogError("CreatorObjectLibrarySO not set!");
                return;
            }
            this.placeableObjectLibrary = placeableObjectLibrary;
            this.creatorObjectLibrary = creatorObjectLibrary;
        }

        public void Setup()
        {
            placeableObjectLibrary.Initialize();
            creatorObjectLibrary.Initialize();
        }
    }
}
