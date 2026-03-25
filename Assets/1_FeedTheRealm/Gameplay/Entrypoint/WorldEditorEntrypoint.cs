using FeedTheRealm.Core.EventChannels.Ticks;
using FeedTheRealm.Gameplay.WorldLoader;
using FeedTheRealm.Gameplay.WorldSetup;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Entrypoint
{
    public class WorldEditorEntrypoint : IStartable, ITickable, IFixedTickable, ILateTickable
    {
        private readonly WorldSetupManager worldSetup;
        private readonly ZoneLoaderManager zoneLoaderManager;
        private readonly TickEvent tickEvent;
        private readonly FixedTickEvent fixedTickEvent;
        private readonly LateTickEvent lateTickEvent;

        public WorldEditorEntrypoint(
            ZoneLoaderManager zoneLoaderManager,
            WorldSetupManager worldSetup,
            TickEvent tickEvent,
            FixedTickEvent fixedTickEvent,
            LateTickEvent lateTickEvent
        )
        {
            this.zoneLoaderManager = zoneLoaderManager;
            this.worldSetup = worldSetup;
            this.tickEvent = tickEvent;
            this.fixedTickEvent = fixedTickEvent;
            this.lateTickEvent = lateTickEvent;
        }

        public async void Start()
        {
            worldSetup.ExecuteSetup();
            await zoneLoaderManager.Load();
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
