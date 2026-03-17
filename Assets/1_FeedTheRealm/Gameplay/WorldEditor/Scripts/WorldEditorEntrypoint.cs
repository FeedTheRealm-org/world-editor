using FeedTheRealm.Core.EventChannels.Ticks;
using FeedTheRealm.Gameplay.WorldSetup;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.WorldEditor
{
    public class WorldEditorEntrypoint : IStartable, ITickable, IFixedTickable, ILateTickable
    {
        private readonly WorldSetupService worldSetup;
        private readonly WorldLoaderManager worldLoaderManager;
        private readonly TickEvent tickEvent;
        private readonly FixedTickEvent fixedTickEvent;
        private readonly LateTickEvent lateTickEvent;

        public WorldEditorEntrypoint(
            WorldLoaderManager worldLoader,
            WorldSetupService worldSetup,
            TickEvent tickEvent,
            FixedTickEvent fixedTickEvent,
            LateTickEvent lateTickEvent
        )
        {
            this.worldLoaderManager = worldLoader;
            this.worldSetup = worldSetup;
            this.tickEvent = tickEvent;
            this.fixedTickEvent = fixedTickEvent;
            this.lateTickEvent = lateTickEvent;
        }

        public void Start()
        {
            worldSetup.ExecuteSetup();
            worldLoaderManager.Load();
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
