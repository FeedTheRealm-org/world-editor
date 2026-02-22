using FeedTheRealm.Gameplay.WorldSetup;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldEditorInitiator : IStartable
    {
        private readonly WorldLoader worldLoader;
        private readonly WorldSetupService worldSetup;

        public WorldEditorInitiator(WorldLoader worldLoader, WorldSetupService worldSetup)
        {
            Debug.Log("WorldEditorInitiator constructed");
            this.worldLoader = worldLoader;
            this.worldSetup = worldSetup;
        }

        public void Start()
        {
            Debug.Log("WorldEditorInitiator Start()");
            worldSetup.Setup();
            worldLoader.Load();
        }
    }
}
