using FeedTheRealm.Core.EventChannels.Ticks;
using FeedTheRealm.Gameplay.MainMenuSetup.Services;
using UnityEngine;
using VContainer.Unity;

namespace FeedTheRealm.Gameplay.MainMenuSetup.Entrypoint
{
    public class MainMenuEntrypoint : IStartable //, ITickable, IFixedTickable, ILateTickable
    {
        private readonly MainMenuUISetupService mainMenuUISetupService;

        public MainMenuEntrypoint(MainMenuUISetupService mainMenuUISetupService)
        {
            this.mainMenuUISetupService = mainMenuUISetupService;
        }

        public void Start()
        {
            mainMenuUISetupService.Setup();
        }
    }
}
