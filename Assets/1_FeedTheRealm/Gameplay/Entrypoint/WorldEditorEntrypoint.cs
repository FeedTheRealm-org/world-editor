using FeedTheRealm.Core.EventChannels.Ticks;
using FeedTheRealm.Gameplay.WorldLoader;
using FeedTheRealm.Gameplay.WorldSetup;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.Entrypoint
{
    public class WorldEditorEntrypoint : IStartable, ITickable, IFixedTickable, ILateTickable
    {
        private readonly WorldSetupManager worldSetup;
        private readonly ZoneLoader zoneLoader;
        private readonly CreatablesLoader creatablesLoader;
        private readonly TickEvent tickEvent;
        private readonly FixedTickEvent fixedTickEvent;
        private readonly LateTickEvent lateTickEvent;

        public WorldEditorEntrypoint(
            ZoneLoader zoneLoader,
            CreatablesLoader creatablesLoader,
            WorldSetupManager worldSetup,
            TickEvent tickEvent,
            FixedTickEvent fixedTickEvent,
            LateTickEvent lateTickEvent
        )
        {
            this.zoneLoader = zoneLoader;
            this.creatablesLoader = creatablesLoader;

            this.worldSetup = worldSetup;
            this.tickEvent = tickEvent;
            this.fixedTickEvent = fixedTickEvent;
            this.lateTickEvent = lateTickEvent;
        }

        public async void Start()
        {
            Application.runInBackground = true;
            worldSetup.ExecuteSetup();
            await creatablesLoader.Load();
            await zoneLoader.Load();
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
