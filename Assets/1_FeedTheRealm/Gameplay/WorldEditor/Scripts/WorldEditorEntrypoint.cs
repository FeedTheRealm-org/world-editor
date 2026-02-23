using FeedTheRealm.Core.EventChannels.Ticks;
using FeedTheRealm.Gameplay.WorldSetup;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldEditorEntrypoint : IStartable, ITickable, IFixedTickable, ILateTickable
    {
        private readonly WorldLoader worldLoader;
        private readonly WorldSetupService worldSetup;
        private readonly TickEvent tickEvent;
        private readonly FixedTickEvent fixedTickEvent;
        private readonly LateTickEvent lateTickEvent;

        public WorldEditorEntrypoint(
            WorldLoader worldLoader,
            WorldSetupService worldSetup,
            TickEvent tickEvent,
            FixedTickEvent fixedTickEvent,
            LateTickEvent lateTickEvent
        )
        {
            this.worldLoader = worldLoader;
            this.worldSetup = worldSetup;
            this.tickEvent = tickEvent;
            this.fixedTickEvent = fixedTickEvent;
            this.lateTickEvent = lateTickEvent;
        }

        public void Start()
        {
            worldSetup.ExecuteSetup();
            worldLoader.Load();
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
