using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldSetup;

namespace FeedTheRealm.Gameplay.WorldSetup
{
    public abstract class SetupService : ISetup
    {
        private readonly WorldSetupEvent setupEvent;

        public SetupService(WorldSetupEvent setupEvent)
        {
            this.setupEvent = setupEvent;
            setupEvent.OnRaised += Setup;
        }

        public void Dispose()
        {
            setupEvent.OnRaised -= Setup;
        }

        public abstract void Setup();
    }
}
