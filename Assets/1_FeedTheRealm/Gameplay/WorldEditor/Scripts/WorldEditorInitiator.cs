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
            this.worldLoader = worldLoader;
            this.worldSetup = worldSetup;
        }

        public void Start()
        {
            worldSetup.Setup();
            worldLoader.Load();
        }
    }
}
