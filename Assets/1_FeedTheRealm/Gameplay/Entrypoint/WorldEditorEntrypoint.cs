using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.Ticks;
using FeedTheRealm.Core.Repository;
using FeedTheRealm.Gameplay.Library.PlaceableObjectsLibrary;
using FeedTheRealm.Gameplay.WorldLoader;
using FeedTheRealm.Gameplay.WorldSetup;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Entrypoint
{
    public class WorldEditorEntrypoint : IStartable, ITickable, IFixedTickable, ILateTickable
    {
        private readonly WorldSetupManager worldSetup;
        private readonly ZoneLoaderManager worldLoaderManager;
        private readonly TickEvent tickEvent;
        private readonly FixedTickEvent fixedTickEvent;
        private readonly LateTickEvent lateTickEvent;

        public WorldEditorEntrypoint(
            ZoneLoaderManager worldLoaderManager,
            WorldSetupManager worldSetup,
            TickEvent tickEvent,
            FixedTickEvent fixedTickEvent,
            LateTickEvent lateTickEvent
        )
        {
            this.worldLoaderManager = worldLoaderManager;
            this.worldSetup = worldSetup;
            this.tickEvent = tickEvent;
            this.fixedTickEvent = fixedTickEvent;
            this.lateTickEvent = lateTickEvent;
        }

        public async void Start()
        {
            worldSetup.ExecuteSetup();
            await worldLoaderManager.Load();
        }

        public void Tick()
        {
            tickEvent.Raise();
        }

        public void FixedTick()
        {
            fixedTickEvent.Raise();
        }

        public void LateTick()
        {
            lateTickEvent.Raise();
        }
    }
}
