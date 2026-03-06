using FeedTheRealm.Core.EventChannels.Ticks;
using FeedTheRealm.UI.MainMenu.Services;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.UI.MainMenu.Entrypoint
{
    public class MainMenuEntrypoint : IStartable //, ITickable, IFixedTickable, ILateTickable
    {
        private readonly MainMenuUISetupService mainMenuUISetupService;
        private readonly TickEvent tickEvent;
        private readonly FixedTickEvent fixedTickEvent;
        private readonly LateTickEvent lateTickEvent;

        public MainMenuEntrypoint(
            MainMenuUISetupService mainMenuUISetupService
        /*TickEvent tickEvent,
        FixedTickEvent fixedTickEvent,
        LateTickEvent lateTickEvent*/
        )
        {
            this.mainMenuUISetupService = mainMenuUISetupService;
            /*this.tickEvent = tickEvent;
            this.fixedTickEvent = fixedTickEvent;
            this.lateTickEvent = lateTickEvent;*/
        }

        public void Start()
        {
            mainMenuUISetupService.Setup();
        }

        /*public void Tick()
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
        }*/
    }
}
